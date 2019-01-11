using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public bool debugInfo = false;

    //temp
    private Transform Follow;
    private Vector3 Offset = Vector3.zero;

    private float maxProjectileZoom = 1.5f;
    private float maxZoomVelocityThreshold = 20f;
    private float projectileZoomSpeedFactor = 0.05f;
    private float referenceZoom = 1f;


    private enum CameraState
    {
        IDLE,
        SMOOTH_MOVE,
        SAFEZONE_BOUNDS,
        FOLLOW,
    }

    private CameraState currentCameraState = CameraState.IDLE;

    // smooth move
    private Vector2 smoothMoveDesiredPosition = Vector2.zero;
    private float smoothMoveTime = 0.0f;
    private float smoothMoveSpeed = 0.0f;

    //
    SafeZoneBounds safeZone;

    // Use this for initialization
    void Start()
    {

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
                movementDone = UpdateSmoothMove();
                break;
            case CameraState.SAFEZONE_BOUNDS:
                movementDone = safeZone.Update(GetComponent<Camera>());
                transform.position -= new Vector3(safeZone.desiredCameraShift.x, safeZone.desiredCameraShift.y);
                break;
            case CameraState.FOLLOW:
                movementDone = UpdateFollow();
                break;
        }

        if (movementDone)
            currentCameraState = CameraState.IDLE;
    }

    public void SmoothMoveToPoint(Vector2 desiredPosition, float time, float speed)
    {
        smoothMoveDesiredPosition = desiredPosition;
        smoothMoveTime = time;
        smoothMoveSpeed = speed;
        currentCameraState = CameraState.SMOOTH_MOVE;
    }

    private bool UpdateSmoothMove()
    {
        if(Vector2.Distance(transform.position, smoothMoveDesiredPosition) < 0.1f)
        {
            transform.position = smoothMoveDesiredPosition;
            return true;
        }

        Vector2 velocity = Vector2.zero;
        //Vector2 newPosition = Vector2.SmoothDamp(transform.position, smoothMoveDesiredPosition, ref velocity, smoothMoveTime, smoothMoveSpeed, Time.deltaTime);
        Vector2 newPosition = Vector2.Lerp(transform.position, smoothMoveDesiredPosition, 0.05f);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

        return false;
    }

    public void FollowWithSafeZone(GameObject follow, Rect zone)
    {
        safeZone = new SafeZoneBounds(follow, zone);
        currentCameraState = CameraState.SAFEZONE_BOUNDS;
    }

    public void UpdateSafeZone()
    {
        safeZone.Update(GetComponent<Camera>());
    }

    public void SimpleFollow(Transform follow)
    {
        Follow = follow;
        Offset = follow.transform.position - transform.position;

        maxProjectileZoom = 0.2f;
        maxZoomVelocityThreshold = 20f;
        projectileZoomSpeedFactor = 0.05f;
        referenceZoom = GetComponent<Camera>().orthographicSize;


        currentCameraState = CameraState.FOLLOW;
    }

    private bool UpdateFollow()
    {
        transform.position = Follow.position + Offset;

        float zoomFactor = Mathf.Lerp(0f, 1f, Follow.GetComponent<Rigidbody2D>().velocity.magnitude / maxZoomVelocityThreshold);
        var desiredCameraSize = referenceZoom - (zoomFactor * maxProjectileZoom * referenceZoom);
        float velocity = 0f;
        GetComponent<Camera>().orthographicSize = Mathf.SmoothDamp(GetComponent<Camera>().orthographicSize, desiredCameraSize, ref velocity, projectileZoomSpeedFactor);
        Debug.Log("zoomFactor: " + zoomFactor.ToString());

        return false;
    }
}

struct SafeZoneBounds
{
    public Vector2 desiredCameraShift;

    private Rect safeZone;
    private GameObject followObject;

    public SafeZoneBounds(GameObject follow, Rect zone)
    {
        followObject = follow;
        safeZone = zone;
        desiredCameraShift = Vector2.zero;
    }

    public bool Update(Camera camera)
    {
        // safe area in world space coordinates
        var worldRect = GetWorldRect(camera);
        desiredCameraShift = Vector2.zero;

        // check if safe area contains follow object
        var bounds = followObject.GetComponent<Collider2D>().bounds;
        if (worldRect.Contains(bounds.min) && worldRect.Contains(bounds.max))
            return false; // within rect

        if (bounds.min.x < worldRect.xMin)
            desiredCameraShift.x = worldRect.xMin - bounds.min.x;
        else if (bounds.max.x > worldRect.xMax)
            desiredCameraShift.x = worldRect.xMax - bounds.max.x;

        if (bounds.min.y < worldRect.yMin)
            desiredCameraShift.y = worldRect.yMin - bounds.min.y;
        else if (bounds.max.y > worldRect.yMax)
            desiredCameraShift.y = worldRect.yMax - bounds.max.y;

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