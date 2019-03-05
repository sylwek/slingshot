using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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
    public GameObject trajectoryMarkerTemplate = null;

    // arm parameters
    public float baseArmRotation = 0.0f;
    public float maxArmRotation = 0.0f;
    public float minShootAngleGap = 10f;
    public float releaseProjectileTime = 0.5f;
    public float armSpeed = 3.0f;
    public int trajectoryLineResolution = 30;
    public Vector2 shootForce = Vector2.one;

    // events
    public UnityEvent onProjectileLaunched = new UnityEvent();

    // members
    private Camera gameCamera;
    private CatapultState catapultState = CatapultState.EMPTY;
    private float timeToResetArm = 2.0f;
    private float currentArmTime = 0.0f;
    private float gravity = -9.8f;
    private float initialShootAngleDegrees = 0f;
    private float releaseProjectileAngleDegress = 0f;
    //private float armSpeedFactor = 1f;
    private Vector2 shootInitialVelocity = Vector2.one;
    private int clonedProjectilesCount = 0;
    private GameObject[] trajectoryMarkers;


    // Use this for initialization
    void Start()
    {
        SpawnProjectile();
        GenerateTrajectoryMarkers();

        gravity = Physics2D.gravity.y * currentProjectile.GetComponent<Rigidbody2D>().gravityScale * currentProjectile.GetComponent<Rigidbody2D>().mass;
        gameCamera = FindObjectOfType<GameManager>().gameCamera;
    }

    // Update is called once per frame
    void Update ()
    {
        switch (catapultState)
        {
            case CatapultState.EMPTY:
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, baseArmRotation);
                SpawnProjectile();
                break;

            case CatapultState.LOADED:
                break;

            case CatapultState.AIMING:
                //update arm angle
                var relativeAimingPoint = GetRelativeAimingPoint();
                if (IsValidPointToInitiateShoot(relativeAimingPoint))
                {
                    UpdateArmAimAngle(relativeAimingPoint);
                    DrawTrajectory();
                }

                if (Input.GetMouseButtonUp(0)) // mouse left button released
                {
                    if (IsValidPointToInitiateShoot(relativeAimingPoint))
                    {
                        catapultState = CatapultState.SHOOTING;
                        initialShootAngleDegrees = catapultArm.transform.rotation.eulerAngles.z;
                        var tensionData = CalculateArmTensionData();
                        releaseProjectileAngleDegress = tensionData.releaseAngleDegress;
                        //armSpeedFactor = tensionData.forceFactor;
                        var projData = CalculateProjectileTrajectoryData(relativeAimingPoint);
                        shootInitialVelocity = projData.initialVelocity;
                    }
                    else
                    {
                        // reset arm
                        catapultState = CatapultState.EMPTY;
                    }

                    ShowTrajectoryMarkers(false);
                }

                break;

            case CatapultState.SHOOTING:
                currentArmTime += Time.deltaTime;
                UpdateArmShootAngle();

                var deltaAngle = Mathf.DeltaAngle(catapultArm.transform.rotation.eulerAngles.z, releaseProjectileAngleDegress);
                if (Mathf.Abs(deltaAngle) < Mathf.Epsilon && (currentProjectile.GetComponent<Projectile>().IsActive() == false))
                {
                    currentProjectile.transform.parent = this.transform.parent;
                    currentProjectile.GetComponent<Projectile>().Activate(true);
                    currentProjectile.GetComponent<Projectile>().ApplyForce(shootInitialVelocity);
                    onProjectileLaunched.Invoke();
                }

                if (currentArmTime >= timeToResetArm)
                {
                    catapultState = CatapultState.EMPTY;
                    currentArmTime = 0.0f;
                }
                break;
        }
    }

    void OnMouseDown()
    {
        if (catapultState == CatapultState.LOADED)
        {
            catapultState = CatapultState.AIMING;
            ShowTrajectoryMarkers(true);
        }
    }

    TensionData CalculateArmTensionData()
    {
        var mouseWorldPos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        var force = Mathf.Abs(catapultArm.transform.position.x - mouseWorldPos.x) / catapultArm.transform.position.x;
        var releaseAngle = Mathf.LerpAngle(catapultArm.transform.rotation.eulerAngles.z, maxArmRotation, Mathf.Clamp01(force));
        return new TensionData(releaseAngle, force);
    }

    Vector2 GetProjectileReleasePoint()
    {
        var tensionData = CalculateArmTensionData();
        var projectileArmDistance = (catapultArm.transform.position - currentProjectile.transform.position).magnitude;
        var adjustedAngle = tensionData.releaseAngleDegress + 180f;
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
        clonedProjectilesCount++;
        currentProjectile = GameObject.Instantiate(Resources.Load("Prefabs/projectile") as GameObject);
        currentProjectile.transform.parent = projectileNode.transform;
        currentProjectile.transform.position = projectileNode.transform.position;
        currentProjectile.GetComponent<Projectile>().Activate(false);
        currentProjectile.name = string.Format("{0}_{1}", currentProjectile.name, clonedProjectilesCount);

        catapultState = CatapultState.LOADED;
    }

    private void GenerateTrajectoryMarkers()
    {
        trajectoryMarkers = new GameObject[trajectoryLineResolution];
        for (int i = 0; i < trajectoryMarkers.Length; ++i)
            trajectoryMarkers[i] = GameObject.Instantiate(trajectoryMarkerTemplate);
    }

    private void UpdateArmAimAngle(Vector3 relativePoint)
    {
        catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(baseArmRotation, maxArmRotation + minShootAngleGap, Mathf.Clamp01(relativePoint.y / catapultArm.transform.position.y)));
    }

    private void UpdateArmShootAngle()
    {
        catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(initialShootAngleDegrees, releaseProjectileAngleDegress, Mathf.Clamp01(currentArmTime * armSpeed /** armSpeedFactor*/)));
    }

    private Vector3 GetRelativeAimingPoint()
    {
        // for now mouse position
        return gameCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    // Check if relative point is on right side of catapult and below catapult arm
    private bool IsValidPointToInitiateShoot(Vector3 point)
    {
        return point.x < catapultArm.transform.position.x // on left side of catapult's arm
            && point.y < catapultArm.transform.position.y; // and below
    }

    //void OnDrawGizmos()
    //{
    //    if (!currentProjectile || catapultState != CatapultState.AIMING || !IsValidPointToInitiateShoot(GetRelativeAimingPoint()))
    //        return;

    //    Gizmos.color = new Color(1, 0, 0, 0.3f);
    //    var trajData = CalculateProjectileTrajectoryData(GetRelativeAimingPoint());
    //    var projectileReleasePoint = GetProjectileReleasePoint();
    //    //Debug.Log("Initial Vel: " + trajData.initialVelocity.ToString());
    //    //Gizmos.DrawSphere(GetProjectileReleasePoint(), .3f);

    //    for (int dotIndex = 0; dotIndex < trajectoryLineResolution; ++dotIndex)
    //    {
    //        float simulationTime = dotIndex / (float)trajectoryLineResolution * trajData.flightTime;
    //        var displacement = trajData.initialVelocity * simulationTime + Vector2.up * gravity * simulationTime * simulationTime / 2f;
    //        var drawPoint = projectileReleasePoint + displacement;
    //        Gizmos.DrawSphere(new Vector3(drawPoint.x, drawPoint.y, gameCamera.nearClipPlane), .1f);
    //    }
    //}

    private void DrawTrajectory()
    {
        var trajData = CalculateProjectileTrajectoryData(GetRelativeAimingPoint());
        var projectileReleasePoint = GetProjectileReleasePoint();

        for (int markerIndex = 0; markerIndex < trajectoryLineResolution; ++markerIndex)
        {
            float simulationTime = markerIndex / (float)trajectoryLineResolution * trajData.flightTime;
            var displacement = trajData.initialVelocity * simulationTime + Vector2.up * gravity * simulationTime * simulationTime / 2f;
            var drawPoint = projectileReleasePoint + displacement;
            trajectoryMarkers[markerIndex].transform.position = drawPoint;
        }
    }

    private void ShowTrajectoryMarkers(bool show)
    {
        foreach (var marker in trajectoryMarkers)
            marker.GetComponent<Renderer>().enabled = show;
    }

    TrajectoryData CalculateProjectileTrajectoryData(Vector3 relativePoint)
    {
        // TODO: take projectile's mass into account
        //var mouseWorldPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        //mouseWorldPosition.z = 0;

        var relativePointToArmPositionOffset = catapultArm.transform.position - relativePoint;
        var adjustedShootForce = new Vector3(relativePointToArmPositionOffset.x * shootForce.x, relativePointToArmPositionOffset.y * shootForce.y, 0f);

        var targetPosition = catapultArm.transform.position + adjustedShootForce;
        var h = adjustedShootForce.y;

        float displacementY = targetPosition.y - GetProjectileReleasePoint().y;
        float displacementX = targetPosition.x - GetProjectileReleasePoint().x;
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
    public readonly float releaseAngleDegress;
    public readonly float forceFactor;

    public TensionData(float releaseAngle, float forceFactor)
    {
        this.releaseAngleDegress = releaseAngle;
        this.forceFactor = forceFactor;
    }
}

