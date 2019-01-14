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

    private CameraState currentCameraState = CameraState.IDLE;
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

        safeZone.DrawGizmos(GetComponent<Camera>());
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentCameraState)
        {
            case CameraState.SMOOTH_MOVE:
                if(smoothMove.Update(transform.position))
                    currentCameraState = CameraState.IDLE;

                transform.position = new Vector3(smoothMove.calculatedPosition.x, smoothMove.calculatedPosition.y, transform.position.z);
                break;
            case CameraState.SAFEZONE_BOUNDS:
                safeZone.Update(CameraComponent);
                transform.position -= new Vector3(safeZone.cameraShift.x, safeZone.cameraShift.y);
                break;
            case CameraState.FREE_ROAM:
                var newPosition = transform.position + freeRoamOrigin - CameraComponent.ScreenToWorldPoint(Input.mousePosition);
                newPosition.z = transform.position.z;
                transform.position = newPosition;
                break;
        }

        if(zoomAdjustment.Update(CameraComponent.orthographicSize))
            CameraComponent.orthographicSize = zoomAdjustment.calculatedCameraSize;

        if (CameraBounds != Rect.zero)
            transform.position += BoundedCameraPosition();
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

    public void AdjustZoom(GameObject velocityScr, float maxZoom, float velocityThreshold, float time, float defaultCameraSize)
    {
        zoomAdjustment = new ZoomAdjustment(velocityScr, maxZoom, velocityThreshold, time, defaultCameraSize);
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

    public void Update(Camera camera)
    {
        // safe area in world space coordinates
        var worldRect = GameCamera.GetCameraWorldRect(camera, safeZone);
        cameraShift = Vector2.zero;

        // check if safe area contains follow object
        var bounds = followObject.GetComponent<Collider2D>().bounds;
        if (worldRect.Contains(bounds.min) && worldRect.Contains(bounds.max))
            return; // within rect

        if (bounds.min.x < worldRect.xMin)
            cameraShift.x = worldRect.xMin - bounds.min.x;
        else if (bounds.max.x > worldRect.xMax)
            cameraShift.x = worldRect.xMax - bounds.max.x;

        if (bounds.min.y < worldRect.yMin)
            cameraShift.y = worldRect.yMin - bounds.min.y;
        else if (bounds.max.y > worldRect.yMax)
            cameraShift.y = worldRect.yMax - bounds.max.y;
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
    public float calculatedCameraSize;
    public bool enabled;

    private GameObject VelocitySource;
    private float maxZoom;
    private float velocityThreshold;
    private float timeFactor;
    private float defaultCameraSize;

    public ZoomAdjustment(GameObject velocityScr, float maxZoom, float velocityThreshold, float time, float defaultCameraSize)
    {
        calculatedCameraSize = defaultCameraSize;
        enabled = false;

        VelocitySource = velocityScr;
        this.maxZoom = maxZoom;
        this.velocityThreshold = velocityThreshold;
        timeFactor = time;
        this.defaultCameraSize = defaultCameraSize;
    }

    public bool Update(float currentCameraSize)
    {
        if (VelocitySource == null)
            return false;

        if (enabled)
        {
            float zoomFactor = Mathf.Lerp(0f, 1f, VelocitySource.GetComponent<Rigidbody2D>().velocity.magnitude / velocityThreshold);
            var adjustedSize = defaultCameraSize + (zoomFactor * maxZoom * defaultCameraSize);
            float velocity = 0f;
            calculatedCameraSize = Mathf.SmoothDamp(currentCameraSize, adjustedSize, ref velocity, timeFactor);
        }
        else
        {
            float velocity = 0f;
            calculatedCameraSize = Mathf.SmoothDamp(currentCameraSize, defaultCameraSize, ref velocity, timeFactor);
            if ((calculatedCameraSize - defaultCameraSize) < 0.05f)
                calculatedCameraSize = defaultCameraSize;
        }

        return true;
    }
}
