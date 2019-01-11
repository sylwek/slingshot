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
    public Vector3 initialCameraPosition = Vector3.zero;
    public Vector2 safeAreaSize = Vector2.zero;
    public Rect safeAreaDeadzone = Rect.zero;
    public float maxProjectileZoom = 1.5f;
    public float maxZoomVelocityThreshold = 20f;
    public float projectileZoomSpeedFactor = 0.05f;


    // Use this for initialization
    void Start()
    {
        gameCamera.GetComponent<GameCamera>().SmoothMoveToPoint(initialCameraPosition, 0.5f, 200.0f);
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
            //gameCamera.SimpleFollow(myCatapult.currentProjectile.transform);
        }

        //gameCamera.UpdateSafeZone(myCatapult.currentProjectile.GetComponent<Collider2D>().bounds);
    }
}
