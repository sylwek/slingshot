using UnityEngine;
using System.Collections;

public class Catapult : MonoBehaviour
{
    //
    private enum CatapultState
    {
        EMPTY,
        LOADED,
        SHOOTING,
    }

    // parameters
    public GameObject projectileNode = null;
    public GameObject currentProjectile = null;
    public GameObject catapultArm = null;

    // arm parameters
    public float baseArmRotation = 0.0f;
    public float releaseArmRotation = 0.0f;
    public float releaseProjectileTime = 0.5f;
    public float armSpeed = 3.0f;
    public int trajectoryLineResolution = 30;
    public Vector2 shootForce = Vector2.one;

    // members
    private CatapultState catapultState = CatapultState.EMPTY;
    private float currentArmTime = 0.0f;
    private float gravity = -9.8f;


    // Use this for initialization
    void Start()
    {
        SpawnProjectile();
        gravity = Physics2D.gravity.y * currentProjectile.GetComponent<Rigidbody2D>().gravityScale;
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetMouseButtonUp(0) == true)
        {
            Shoot();
        }

        var trajData = CalculateProjectileTrajectoryData();
        Debug.Log("InitialVel: " + trajData.ToString());

        switch (catapultState)
        {
            case CatapultState.EMPTY:
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, baseArmRotation);
                SpawnProjectile();
                break;

            case CatapultState.LOADED:
                if (Input.GetMouseButtonUp(0) == true)
                {
                    //catapultState = CatapultState.SHOOTING;
                    currentProjectile.GetComponent<Projectile>().Activate(true);
                    currentProjectile.GetComponent<Projectile>().ApplyVelocity(trajData.initialVelocity);
                }

                currentArmTime = 0.0f;
                break;

            case CatapultState.SHOOTING:
                currentArmTime += Time.deltaTime;
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(baseArmRotation, releaseArmRotation, Mathf.Clamp01(currentArmTime * armSpeed)));

                if ((currentArmTime >= releaseProjectileTime) && (currentProjectile.GetComponent<Projectile>().IsActive() == false))
                {
                    currentProjectile.transform.parent = this.transform.parent;
                    currentProjectile.GetComponent<Projectile>().Activate(true);
                    currentProjectile.GetComponent<Projectile>().ApplyVelocity(new Vector2(50.0f, 20.0f));
                }

                if (currentArmTime >= 2.0f)
                {
                    catapultState = CatapultState.EMPTY;
                }
                break;
        }
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

    //
    private void Shoot()
    {

    }

    void OnDrawGizmos()
    {
        if (!currentProjectile)
            return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        var trajData = CalculateProjectileTrajectoryData();

        for (int dotIndex = 0; dotIndex < trajectoryLineResolution; ++dotIndex)
        {
            float simulationTime = dotIndex / (float)trajectoryLineResolution * trajData.flightTime;
            var displacement = trajData.initialVelocity * simulationTime + Vector2.up * gravity * simulationTime * simulationTime / 2f;
            var drawPoint = new Vector2(currentProjectile.transform.position.x, currentProjectile.transform.position.y) + displacement;
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
        //var gravity = Physics2D.gravity.y;

        float displacementY = targetPosition.y - currentProjectile.transform.position.y;
        float displacementX = targetPosition.x - currentProjectile.transform.position.x;
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
