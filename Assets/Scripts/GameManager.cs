using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    //
    public bool debugInfo = false;
    public GameCamera gameCamera = null;
    public GameObject parralaxManager = null;
    public Catapult myCatapult = null;
    public GameObject myCastle = null;
    public Transform initialCameraPosition = null;
    public Rect safeAreaDeadzone = Rect.zero;
    public float maxProjectileZoom = 0.5f;
    public float maxZoomVelocityThreshold = 20f;
    public float projectileZoomSpeedFactor = 0.05f;
    //shake
    public float ShakeDuration = 0.2f;
    public float ShakePower = 1.0f;
    public Shaker CameraHolder;
    //
    public float NextShootDelayTime = 2f;
    public float NextShootCameraMovementSpeed = 2f;
    private float NextShootCountdown = 0f;
    // camera bounds
    Rect CameraBounds = new Rect(0, 0, 100, 40);


    // Use this for initialization
    void Start()
    {
        // setup initial camera's position
        //gameCamera.SmoothMoveToPoint(initialCameraPosition.position, 0.1f);
        gameCamera.SetCameraBounds(CameraBounds);
    }

    private void OnDrawGizmos()
    {
        if (!debugInfo)
            return;

        Gizmos.color = new Color(1, 0, 0, .5f);
        var camera = gameCamera.GetComponent<Camera>();

        float height = camera.orthographicSize * 2.0f;
        float width = height * camera.aspect;

        Gizmos.DrawCube(camera.ViewportToWorldPoint(
            new Vector3(safeAreaDeadzone.center.x, safeAreaDeadzone.center.y, camera.nearClipPlane)),
            new Vector2(width * safeAreaDeadzone.width, height * safeAreaDeadzone.height)
            );
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) == true)
        {
            gameCamera.FollowWithSafeZone(myCatapult.currentProjectile, safeAreaDeadzone);
            gameCamera.AdjustZoom(myCatapult.currentProjectile, maxProjectileZoom, maxZoomVelocityThreshold, projectileZoomSpeedFactor);
        }

        if(NextShootCountdown > 0f)
        {
            NextShootCountdown -= Time.deltaTime;

            if(NextShootCountdown <= 0f)
            {
                NextShootCountdown = 0f;
                SetupNextShoot();
            }
        }
    }

    public void OnFirstProjectileCollision()
    {
        CameraHolder.Shake(ShakeDuration, ShakePower);
    }

    public void StartCountdownToNextShoot()
    {
        Debug.Log("StartCountdownToNextShoot");
        NextShootCountdown = NextShootDelayTime;
    }

    private void SetupNextShoot()
    {
        gameCamera.SmoothMoveToPoint(initialCameraPosition.position, NextShootCameraMovementSpeed);
        gameCamera.DisableZoomAdjustment();
    }
}
