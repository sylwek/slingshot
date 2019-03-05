using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject SceneDragDetectionObject;

    private GameCamera gameCameraComponent;
    private float defaultCameraSize;

    // Use this for initialization
    void Start()
    {
        gameCameraComponent = gameManager.gameCamera.GetComponent<GameCamera>();
        gameCameraComponent.SetCameraBounds(gameManager.CameraBounds);

        defaultCameraSize = gameManager.gameCamera.orthographicSize;

        SetupDragEventTriggers();

        //gameManager.onNextShootSetup.AddListener(() => { SmoothMoveToInitialPosition(); });
        gameManager.onNextShootSetup.AddListener(SmoothMoveToInitialPosition);
        gameManager.myCatapult.onProjectileLaunched.AddListener(OnProjectileLaunched);
    }

    private void SetupDragEventTriggers()
    {
        var trigger = SceneDragDetectionObject.GetComponent<EventTrigger>();

        var dragBeginEntry = new EventTrigger.Entry();
        dragBeginEntry.eventID = EventTriggerType.BeginDrag;
        dragBeginEntry.callback.AddListener((data) => { gameCameraComponent.EnableFreeRoam(gameManager.gameCamera.ScreenToWorldPoint(Input.mousePosition)); });
        trigger.triggers.Add(dragBeginEntry);

        var dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.eventID = EventTriggerType.EndDrag;
        dragEndEntry.callback.AddListener((data) => { gameCameraComponent.DisableFreeRoam(); });
        trigger.triggers.Add(dragEndEntry);
    }

    private void OnProjectileLaunched()
    {
        gameCameraComponent.FollowWithSafeZone(gameManager.myCatapult.currentProjectile, gameManager.safeAreaDeadzone);
        gameCameraComponent.AdjustZoom(
            gameManager.myCatapult.currentProjectile,
            gameManager.maxFlightZoomPercentage,
            gameManager.maxZoomVelocityThreshold,
            gameManager.zoomTimeFactor,
            defaultCameraSize);
    }

    private void SmoothMoveToInitialPosition()
    {
        gameCameraComponent.SmoothMoveToPoint(gameManager.initialCameraPosition.position, gameManager.NextShootCameraMovementSpeed);
        gameCameraComponent.DisableZoomAdjustment();
    }
}
