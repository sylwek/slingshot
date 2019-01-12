using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public bool debugInfo = false;

    private enum CameraState
    {
        FREE_ROAM,
        SMOOTH_MOVE,
        SAFEZONE_BOUNDS,
        IDLE,
    }

    private CameraState currentCameraState = CameraState.FREE_ROAM;
    private Camera CameraComponent = null;
    private Rect CameraBounds = Rect.zero;

    // camera's modifiers
    SafeZoneBounds safeZone;
    SmoothMove smoothMove;
    ZoomAdjustment zoomAdjustment;

    // free roam
    Vector3 freeRoamOrigin = Vector3.zero;

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
                transform.position = new Vector3(smoothMove.calculatedPosition.x, smoothMove.calculatedPosition.y, transform.position.z);
                break;
            case CameraState.SAFEZONE_BOUNDS:
                movementDone = safeZone.Update(CameraComponent);
                transform.position -= new Vector3(safeZone.cameraShift.x, safeZone.cameraShift.y);
                break;
            case CameraState.FREE_ROAM:
                transform.position = transform.position + freeRoamOrigin - CameraComponent.ScreenToWorldPoint(Input.mousePosition);
                break;
        }

        if (!zoomAdjustment.Update(CameraComponent.orthographicSize))
            CameraComponent.orthographicSize = zoomAdjustment.zoomedOrthCameraSize;

        if (CameraBounds != Rect.zero)
            transform.position += BoundedCameraPosition();

        if (movementDone)
            currentCameraState = CameraState.IDLE;
    }

    public static Rect GetCameraWorldRect(Camera camera, Rect viewportRect)
    {
        float height = camera.orthographicSize * 2.0f;
        float width = height * camera.aspect;
        var worldRect = new Rect(
            camera.ViewportToWorldPoint(new Vector2(viewportRect.x, viewportRect.y)),
            new Vector2(width * viewportRect.width, height * viewportRect.height)
            );

        return worldRect;
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

    public void SetCameraBounds(Rect bounds)
    {
        CameraBounds = bounds;
    }

    public void EnableFreeRoam(Vector3 origin)
    {
        freeRoamOrigin = origin;
        currentCameraState = CameraState.FREE_ROAM;
    }

    public void DisableFreeRoam()
    {
        currentCameraState = CameraState.IDLE;
    }

    private Vector3 BoundedCameraPosition()
    {
        var worldRect = GameCamera.GetCameraWorldRect(CameraComponent, new Rect(0f, 0f, 1f, 1f));
        var cameraShift = Vector3.zero;
        if (CameraBounds.xMin > worldRect.xMin)
            cameraShift.x = CameraBounds.xMin - worldRect.xMin;
        else if (CameraBounds.xMax < worldRect.xMax)
            cameraShift.x = CameraBounds.xMax - worldRect.xMax;

        if (CameraBounds.yMin > worldRect.yMin)
            cameraShift.y = CameraBounds.yMin - worldRect.yMin;
        else if (CameraBounds.yMax < worldRect.yMax)
            cameraShift.y = CameraBounds.yMax - worldRect.yMax;

        //if (cameraShift != Vector3.zero)
        //    Debug.Log("Boundary");

        return cameraShift;
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
        var worldRect = GameCamera.GetCameraWorldRect(camera, safeZone);
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
        var worldRect = GameCamera.GetCameraWorldRect(camera, safeZone);
        Gizmos.DrawCube(worldRect.center, worldRect.size);
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
