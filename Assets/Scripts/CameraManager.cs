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

    }

    public void OnDragBegin(PointerEventData data)
    {
        gameCamera.GetComponent<GameCamera>().EnableFreeRoam(gameCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    public void OnDrag(PointerEventData data)
    {

    }

    public void OnDragEnd(PointerEventData data)
    {
        gameCamera.GetComponent<GameCamera>().EnableFreeRoam(Vector3.zero);
    }

    private void SetupDragEventTriggers()
    {
        var trigger = SceneDragDetectionObject.GetComponent<EventTrigger>();

        var dragBeginEntry = new EventTrigger.Entry();
        dragBeginEntry.eventID = EventTriggerType.BeginDrag;
        dragBeginEntry.callback.AddListener((data) => { OnDragBegin((PointerEventData)data); });
        trigger.triggers.Add(dragBeginEntry);

        //var dragEntry = new EventTrigger.Entry();
        //dragEntry.eventID = EventTriggerType.Drag;
        //dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        //trigger.triggers.Add(dragEntry);

        var dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.eventID = EventTriggerType.EndDrag;
        dragEndEntry.callback.AddListener((data) => { OnDragEnd((PointerEventData)data); });
        trigger.triggers.Add(dragEndEntry);
    }
}
