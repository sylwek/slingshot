using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    //
    public bool drawDebugInfo = false;
    public GameObject parralaxManager = null;
    public Catapult myCatapult = null;
    public GameObject myCastle = null;

    [Header("Camera Settings")]
    public Camera gameCamera = null;
    public Transform initialCameraPosition = null;
    public Rect CameraBounds = new Rect(0, 0, 100, 40);
    public Rect safeAreaDeadzone = Rect.zero;

    [Header("Projectile Zoom")]
    public float maxFlightZoomPercentage = 0.25f;
    public float maxZoomVelocityThreshold = 20f;
    public float zoomTimeFactor = 0.05f;

    [Header("Shake")]
    public Shaker cameraHolder;
    public float shakeDuration = 0.2f;
    public float shakePower = 1.0f;

    [Header("Game Flow")]
    public float NextShootDelayTime = 2f;
    public float NextShootCameraMovementSpeed = 2f;
    private bool nextShootCoroutineExecuting = false;

    [Header("Events")]
    public UnityEvent onNextShootSetup = new UnityEvent();

    void OnDrawGizmos()
    {
        if (!drawDebugInfo)
            return;

        Gizmos.color = new Color(1, 0, 0, .25f);
        var camera = gameCamera.GetComponent<Camera>();

        float height = camera.orthographicSize * 2.0f;
        float width = height * camera.aspect;

        Gizmos.DrawCube(camera.ViewportToWorldPoint(
            new Vector3(safeAreaDeadzone.center.x, safeAreaDeadzone.center.y, camera.nearClipPlane)),
            new Vector2(width * safeAreaDeadzone.width, height * safeAreaDeadzone.height)
            );
    }

    public void OnFirstProjectileCollision()
    {
        cameraHolder.Shake(shakeDuration, shakePower);
    }

    public void StartCountdownToInitNextShoot()
    {
        Debug.Log("StartCountdownToNextShoot");
        StartCoroutine(SetupNextShootAfterTime(NextShootDelayTime));
    }

    IEnumerator SetupNextShootAfterTime(float time)
    {
        if (nextShootCoroutineExecuting)
            yield break;

        nextShootCoroutineExecuting = true;

        yield return new WaitForSeconds(time);

        SetupNextShoot();

        nextShootCoroutineExecuting = false;
    }

    void SetupNextShoot()
    {
        Debug.Log("SetupNextShoot");
        onNextShootSetup.Invoke();
    }
}
