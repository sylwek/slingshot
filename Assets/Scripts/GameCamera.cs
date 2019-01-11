using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public bool debugInfo = false;

    private enum CameraState
    {
        IDLE,
        SMOOTH_MOVE,
        SAFEZONE_BOUNDS,
    }

    private CameraState currentCameraState = CameraState.IDLE;
    private Camera CameraComponent = null;

    // camera's modifiers
    SafeZoneBounds safeZone;
    SmoothMove smoothMove;
    ZoomAdjustment zoomAdjustment;

    // Use this for initialization
    void Start()
    {
        CameraComponent = GetComponent<Camera>();
        //if (CameraComponent == null)
        //    CameraComponent = GetComponentInChildren<Camera>();
    }

    private void OnDrawGizmos()
    {
        if (!debugInfo)
            return;
        //Gizmos.color = new Color(0, 0, 1, .5f);
        //var camera = GetComponent<Camera>();

        //float height = camera.orthographicSize * 2.0f;
        //float width = height * camera.aspect;

        //Gizmos.DrawCube(camera.ViewportToWorldPoint(
        //    new Vector3(0.5f, 0.5f, camera.nearClipPlane)),
        //    new Vector2(width / 2, height / 2)
        //    );

        safeZone.DrawGizmos(GetComponent<Camera>());
    }

    // Update is called once per frame
    void Update()
    {
        bool movementDone = false;
        switch (currentCameraState)
        {
            case CameraState.SMOOTH_MOVE:
                movementDone = smoothMove.Update(transform.position);
                transform.position = smoothMove.calculatedPosition;
                break;
            case CameraState.SAFEZONE_BOUNDS:
                movementDone = safeZone.Update(CameraComponent);
                transform.position -= new Vector3(safeZone.cameraShift.x, safeZone.cameraShift.y);
                break;
        }

        if (!zoomAdjustment.Update(CameraComponent.orthographicSize))
            CameraComponent.orthographicSize = zoomAdjustment.zoomedOrthCameraSize;

        if (movementDone)
            currentCameraState = CameraState.IDLE;
    }

    public void SmoothMoveToPoint(Vector2 desiredPosition, float speed)
    {
        smoothMove = new SmoothMove(desiredPosition, speed);
        currentCameraState = CameraState.SMOOTH_MOVE;
    }

    public void FollowWithSafeZone(GameObject follow, Rect zone)
    {
        safeZone = new SafeZoneBounds(follow, zone);
        currentCameraState = CameraState.SAFEZONE_BOUNDS;
    }

    public void AdjustZoom(GameObject velocityScr, float maxZoom, float velocityThreshold, float speed)
    {
        zoomAdjustment = new ZoomAdjustment(velocityScr, maxZoom, velocityThreshold, speed, CameraComponent.orthographicSize);
        zoomAdjustment.enabled = true;
    }

    public void DisableZoomAdjustment()
    {
        zoomAdjustment.enabled = false;
    }
}

struct SafeZoneBounds
{
    public Vector2 cameraShift;

    private Rect safeZone;
    private GameObject followObject;

    public SafeZoneBounds(GameObject follow, Rect zone)
    {
        followObject = follow;
        safeZone = zone;
        cameraShift = Vector2.zero;
    }

    public bool Update(Camera camera)
    {
        // safe area in world space coordinates
        var worldRect = GetWorldRect(camera);
        cameraShift = Vector2.zero;

        // check if safe area contains follow object
        var bounds = followObject.GetComponent<Collider2D>().bounds;
        if (worldRect.Contains(bounds.min) && worldRect.Contains(bounds.max))
            return false; // within rect

        if (bounds.min.x < worldRect.xMin)
            cameraShift.x = worldRect.xMin - bounds.min.x;
        else if (bounds.max.x > worldRect.xMax)
            cameraShift.x = worldRect.xMax - bounds.max.x;

        if (bounds.min.y < worldRect.yMin)
            cameraShift.y = worldRect.yMin - bounds.min.y;
        else if (bounds.max.y > worldRect.yMax)
            cameraShift.y = worldRect.yMax - bounds.max.y;

        return false;
    }

    public void DrawGizmos(Camera camera)
    {
        Gizmos.color = new Color(0, 0, 1, .5f);
        var worldRect = GetWorldRect(camera);
        Gizmos.DrawCube(worldRect.center, worldRect.size);
    }

    Rect GetWorldRect(Camera camera)
    {
        float height = camera.orthographicSize * 2.0f;
        float width = height * camera.aspect;
        var safeZoneWorldRect = new Rect(
            camera.ViewportToWorldPoint(new Vector2(safeZone.x, safeZone.y)),
            new Vector2(width * safeZone.width, height * safeZone.height)
            );

        return safeZoneWorldRect;
    }
}

struct SmoothMove
{
    public Vector2 calculatedPosition;

    private Vector2 desiredPosition;
    private float speed;

    public SmoothMove(Vector2 position, float moveSpeed)
    {
        calculatedPosition = Vector2.zero;
        desiredPosition = position;
        speed = moveSpeed;
    }

    public bool Update(Vector2 currentCameraPosition)
    {
        if (Vector2.Distance(currentCameraPosition, desiredPosition) < 0.05f)
        {
            calculatedPosition = desiredPosition;
            Debug.Log("SmoothMove ended.");
            return true;
        }

        Vector2 velocity = Vector2.zero;
        //calculatedPosition = Vector2.SmoothDamp(transform.position, smoothMoveDesiredPosition, ref velocity, smoothMoveTime, smoothMoveSpeed, Time.deltaTime);
        calculatedPosition = Vector2.Lerp(currentCameraPosition, desiredPosition, speed);

        return false;
    }
}

struct ZoomAdjustment
{
    public float zoomedOrthCameraSize;

    private float maxProjectileZoom;
    private float maxZoomVelocityThreshold;
    private float projectileZoomSpeedFactor;
    private float referenceCameraSize;
    private GameObject VelocitySource;
    public bool enabled { get; set; }

    public ZoomAdjustment(GameObject velocityScr, float maxZoom, float velocityThreshold, float speed, float cameraSize)
    {
        zoomedOrthCameraSize = cameraSize;
        VelocitySource = velocityScr;
        maxProjectileZoom = maxZoom;
        maxZoomVelocityThreshold = velocityThreshold;
        projectileZoomSpeedFactor = speed;
        referenceCameraSize = cameraSize;
        enabled = false;
    }

    public bool Update(float currentCameraSize)
    {
        if (VelocitySource == null)
            return true;

        if (enabled)
        {
            float zoomFactor = Mathf.Lerp(0f, 1f, VelocitySource.GetComponent<Rigidbody2D>().velocity.magnitude / maxZoomVelocityThreshold);
            var adjustedSize = referenceCameraSize - (zoomFactor * maxProjectileZoom * referenceCameraSize);
            float velocity = 0f;
            zoomedOrthCameraSize = Mathf.SmoothDamp(currentCameraSize, adjustedSize, ref velocity, projectileZoomSpeedFactor);
            return false;
        }
        else
        {
            float velocity = 0f;
            zoomedOrthCameraSize = Mathf.SmoothDamp(currentCameraSize, referenceCameraSize, ref velocity, projectileZoomSpeedFactor);
            bool done = (zoomedOrthCameraSize - referenceCameraSize) < 0.05f;

            if (done)
                zoomedOrthCameraSize = referenceCameraSize;

            return done;
        }
    }
}
