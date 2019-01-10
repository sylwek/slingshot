using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    private enum CameraState
    {
        IDLE,
        SMOOTH_MOVE,
        SAFEZONE_BOUNDS,
    }

    private CameraState currentCameraState = CameraState.IDLE;

    // smooth move
    private Vector2 smoothMoveDesiredPosition = Vector2.zero;
    private float smoothMoveTime = 0.0f;
    private float smoothMoveSpeed = 0.0f;

    //
    SafeZoneBounds safeZone;

    struct SafeZoneBounds
    {
        public Vector2 areaSize;
        public Rect boundaryRect;
        public Bounds followBounds;
        //float left, right, top, bottom;

        public SafeZoneBounds(Bounds targetBounds, Vector2 size)
        {
            areaSize = size;
            boundaryRect = new Rect(targetBounds.center, size);
            followBounds = targetBounds;
        }

        public bool Update(Bounds bounds)
        {
            boundaryRect = new Rect(bounds.center, areaSize);
            followBounds = bounds;
            return false;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, .5f);
        //Gizmos.DrawCube(safeZone.boundaryRect.center, safeZone.areaSize);
        Gizmos.DrawCube(safeZone.followBounds.center, safeZone.areaSize);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool movementDone = false;
        switch(currentCameraState)
        {
            case CameraState.SMOOTH_MOVE:
                movementDone = UpdateSmoothMove();
                break;
            case CameraState.SAFEZONE_BOUNDS:
                //movementDone = safeZone.Update();
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

    public void FollowWithSafeZone(Bounds targetBounds, Vector2 size)
    {
        safeZone = new SafeZoneBounds(targetBounds, size);
        currentCameraState = CameraState.SAFEZONE_BOUNDS;
    }

    public void UpdateSafeZone(Bounds bounds)
    {
        safeZone.Update(bounds);
    }
}
