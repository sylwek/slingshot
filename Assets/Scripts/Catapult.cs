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

    // members
    private CatapultState catapultState = CatapultState.EMPTY;
    private float currentArmTime = 0.0f;


	// Use this for initialization
	void Start ()
    {
        SpawnProjectile();
	}

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonUp(0) == true)
        {
            Shoot();
        }

        switch (catapultState)
        {
            case CatapultState.EMPTY:
                catapultArm.transform.rotation = Quaternion.Euler(0f, 0f, baseArmRotation);
                SpawnProjectile();
                break;

            case CatapultState.LOADED:
                //if (Input.GetMouseButtonUp(0) == true)
                //    catapultState = CatapultState.SHOOTING;

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
}
