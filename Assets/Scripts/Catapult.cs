using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class Catapult : MonoBehaviour
{
    //
    private enum CatapultState
    {
        EMPTY,
        LOADED,
        AIMING,
        SHOOTING,
    }

    // parameters
    public GameObject projectileNode = null;
    public GameObject currentProjectile = null;
    public GameObject catapultArm = null;

    // arm parameters
    public float baseArmRotation = 0.0f;
    public float maxArmRotation = 0.0f;
    public float minShootAngleGap = 10f;
    public float releaseProjectileTime = 0.5f;
    public float armSpeed = 3.0f;
    public int trajectoryLineResolution = 30;
    public Vector2 shootForce = Vector2.one;

    // members
    private CatapultState catapultState = CatapultState.EMPTY;
    private float currentArmTime = 0.0f;
    private float gravity = -9.8f;
    private float shootAngle = 0f;
    private float releaseProjectileAngle = 0f;
    private float armSpeedFactor = 1f;
    private Vector2 shootInitialVelocity = Vector2.one;


    // Use this for initialization
    void Start()
    {
        SpawnProjectile();
        gravity = Physics2D.gravity.y * currentProjectile.GetComponent<Rigidbody2D>().gravityScale;
    }

    private void OnMouseDown()
    {
        if (catapultState == CatapultState.LOADED)
            catapultState = CatapultState.AIMING;
    }

    // Update is called once per frame
    void Update ()
    {
        //if (Input.GetMouseButtonUp(0) == true)
        //{
        //    Shoot();
        //}

        //var trajData = CalculateProjectileTrajectoryData();
        //Debug.Log("InitialVel: " + trajData.ToString());

        switch (catapultState)
        {
            case CatapultState.EMPTY:
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, baseArmRotation);
                SpawnProjectile();
                break;

            case CatapultState.LOADED:
                //if (Input.GetMouseButtonUp(0) == true)
                //{
                //    //catapultState = CatapultState.SHOOTING;
                //    currentProjectile.GetComponent<Projectile>().Activate(true);
                //    currentProjectile.GetComponent<Projectile>().ApplyVelocity(trajData.initialVelocity);
                //}

                //currentArmTime = 0.0f;
                break;

            case CatapultState.AIMING:
                //update arm angle
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(baseArmRotation, maxArmRotation + minShootAngleGap, Mathf.Clamp01(mouseWorldPos.y / catapultArm.transform.position.y)));

                if (Input.GetMouseButtonUp(0) == true)
                {
                    catapultState = CatapultState.SHOOTING;
                    shootAngle = catapultArm.transform.rotation.eulerAngles.z;
                    var tensionData = CalculateTensionData();
                    releaseProjectileAngle = tensionData.releaseAngle;
                    armSpeedFactor = tensionData.forceFactor;
                    var projData = CalculateProjectileTrajectoryData();
                    shootInitialVelocity = projData.initialVelocity;
                }

                break;

            case CatapultState.SHOOTING:
                currentArmTime += Time.deltaTime;
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(shootAngle, releaseProjectileAngle, Mathf.Clamp01(currentArmTime * armSpeed /** armSpeedFactor*/)));

                if (/*(currentArmTime >= releaseProjectileTime)*/ Mathf.DeltaAngle(catapultArm.transform.rotation.eulerAngles.z, releaseProjectileAngle) < 0.01f
                    && (currentProjectile.GetComponent<Projectile>().IsActive() == false))
                {
                    currentProjectile.transform.parent = this.transform.parent;
                    currentProjectile.GetComponent<Projectile>().Activate(true);
                    currentProjectile.GetComponent<Projectile>().ApplyVelocity(shootInitialVelocity);
                }

                if (currentArmTime >= 2.0f)
                {
                    catapultState = CatapultState.EMPTY;
                    currentArmTime = 0.0f;
                }
                break;
        }
    }

    TensionData CalculateTensionData()
    {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var force = Mathf.Abs(catapultArm.transform.position.x - mouseWorldPos.x) / catapultArm.transform.position.x;
        var releaseAngle = Mathf.LerpAngle(catapultArm.transform.rotation.eulerAngles.z, maxArmRotation, Mathf.Clamp01(force));
        return new TensionData(releaseAngle, force);
    }

    Vector2 GetProjectileReleasePoint()
    {
        var tensionData = CalculateTensionData();
        var projectileArmDistance = (catapultArm.transform.position - currentProjectile.transform.position).magnitude;
        //var position = catapultArm.transform.position + Quaternion.Euler(0, 0, tensionData.releaseAngle) * projectileArmDistance
        //Vector3 newFwd = Quaternion.Euler(0, 0, tensionData.releaseAngle) * Vector2.zero;
        var adjustedAngle = tensionData.releaseAngle + 180f;
        return catapultArm.transform.position + new Vector3(
            projectileArmDistance * Mathf.Cos(adjustedAngle * Mathf.Deg2Rad),
            projectileArmDistance * Mathf.Sin(adjustedAngle * Mathf.Deg2Rad),
            0f
            );
    }

    Transform GetTensionReferencePoint()
    {
        return catapultArm.transform;
    }

    //
    private void SpawnProjectile()
    {
        currentProjectile = GameObject.Instantiate(Resources.Load("Prefabs/projectile") as GameObject);
        currentProjectile.transform.parent = projectileNode.transform;
        currentProjectile.transform.position = projectileNode.transform.position;
        currentProjectile.GetComponent<Projectile>().Activate(false);

        catapultState = CatapultState.LOADED;
    }

    void OnDrawGizmos()
    {
        if (!currentProjectile || catapultState != CatapultState.AIMING)
            return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        var trajData = CalculateProjectileTrajectoryData();
        Debug.Log("Initial Vel: " + trajData.initialVelocity.ToString());
        Gizmos.DrawSphere(GetProjectileReleasePoint(), .3f);
        for (int dotIndex = 0; dotIndex < trajectoryLineResolution; ++dotIndex)
        {
            float simulationTime = dotIndex / (float)trajectoryLineResolution * trajData.flightTime;
            var displacement = trajData.initialVelocity * simulationTime + Vector2.up * gravity * simulationTime * simulationTime / 2f;
            var drawPoint = /*new Vector2(currentProjectile.transform.position.x, currentProjectile.transform.position.y)*/GetProjectileReleasePoint() + displacement;
            Gizmos.DrawSphere(new Vector3(drawPoint.x, drawPoint.y, Camera.main.nearClipPlane), .1f);
        }
    }

    TrajectoryData CalculateProjectileTrajectoryData()
    {
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        var mouseArmDiff = catapultArm.transform.position - mouseWorldPosition;
        var adjustedShootForce = new Vector3(mouseArmDiff.x * shootForce.x, mouseArmDiff.y * shootForce.y, 0f);

        var targetPosition = catapultArm.transform.position + adjustedShootForce;
        var h = adjustedShootForce.y;

        float displacementY = targetPosition.y - /*currentProjectile.transform.position.y*/GetProjectileReleasePoint().y;
        float displacementX = targetPosition.x - /*currentProjectile.transform.position.x*/GetProjectileReleasePoint().x;
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector2 velocityY = Vector2.up * Mathf.Sqrt(-2 * gravity * h);
        Vector2 velocityX = new Vector2(displacementX / time, 0f);

        return new TrajectoryData(velocityX + velocityY * -Mathf.Sign(gravity), time);
    }
}

struct TrajectoryData
{
    public readonly Vector2 initialVelocity;
    public readonly float flightTime;

    public TrajectoryData(Vector2 initialVelocity, float flightTime)
    {
        this.initialVelocity = initialVelocity;
        this.flightTime = flightTime;
    }
}

struct TensionData
{
    public readonly float releaseAngle;
    public readonly float forceFactor;

    public TensionData(float releaseAngle, float forceFactor)
    {
        this.releaseAngle = releaseAngle;
        this.forceFactor = forceFactor;
    }
}

