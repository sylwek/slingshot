using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public Camera gameCamera;
    public GameObject SceneDragDetectionObject;

    private Vector2 lastDragPosition = Vector3.zero;
    private bool dragging = false;

    // Use this for initialization
    void Start()
    {
        SetupDragEventTriggers();
    }

    // Update is called once per frame
    void Update()
    {
        //if(dragging)
        //{
        //    var worldDragPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        //    gameCamera.GetComponent<GameCamera>().UpdateFreeRoam(worldDragPosition - lastDragPosition);
        //    Debug.Log("MousePos: " + Input.mousePosition.ToString() + "Offset: " + (worldDragPosition - lastDragPosition).ToString());
        //    lastDragPosition = worldDragPosition;
        //}
    }

    public void OnDragBegin(PointerEventData data)
    {
        //lastDragPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        lastDragPosition = data.position;
        dragging = true;
    }

    public void OnDrag(PointerEventData data)
    {
        //var screenOffset = (data.position - lastDragPosition);
        //var screenOffsetNorm = new Vector2(screenOffset.x / Screen.width, screenOffset.y / Screen.height);
        var current = gameCamera.ScreenToViewportPoint(data.position);
        var last = gameCamera.ScreenToViewportPoint(lastDragPosition);
        var change = current - last;
        float height = gameCamera.orthographicSize * 2.0f;
        float width = height * gameCamera.aspect;
        gameCamera.GetComponent<GameCamera>().UpdateFreeRoam(new Vector3(change.x * width, change.y * height));

        //var worldDragPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        //gameCamera.GetComponent<GameCamera>().UpdateFreeRoam(worldDragPosition - lastDragPosition);
        //Debug.Log("MousePos: " + Input.mousePosition.ToString() + "Offset: " + (worldDragPosition - lastDragPosition).ToString());
        //lastDragPosition = worldDragPosition;

    }

    public void OnDragEnd(PointerEventData data)
    {
        //Debug.Log("Drag End");
        dragging = false;
    }

    private void SetupDragEventTriggers()
    {
        var trigger = SceneDragDetectionObject.GetComponent<EventTrigger>();

        var dragBeginEntry = new EventTrigger.Entry();
        dragBeginEntry.eventID = EventTriggerType.BeginDrag;
        dragBeginEntry.callback.AddListener((data) => { OnDragBegin((PointerEventData)data); });
        trigger.triggers.Add(dragBeginEntry);

        var dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(dragEntry);

        var dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.eventID = EventTriggerType.EndDrag;
        dragEndEntry.callback.AddListener((data) => { OnDragEnd((PointerEventData)data); });
        trigger.triggers.Add(dragEndEntry);
    }
}
