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


    // Use this for initialization
    void Start()
    {
        gameCamera.GetComponent<GameCamera>().SmoothMoveToPoint(initialCameraPosition, 0.5f, 200.0f);
    }

    private void OnDrawGizmos()
    {
        if (!debugInfo)
            return;

        //var worldPosLeftTop = gameCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector2(safeAreaDeadzone.xMin * Screen.width, safeAreaDeadzone.yMin * Screen.height));
        //var worldPosBottomRight = gameCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector2(safeAreaDeadzone.xMax * Screen.width, safeAreaDeadzone.yMax * Screen.height));

        var position = gameCamera.GetComponent<Camera>().ScreenToViewportPoint(new Vector2(safeAreaDeadzone.center.x * Screen.width, safeAreaDeadzone.center.y * Screen.height));
        var size = /*gameCamera.GetComponent<Camera>().ScreenToWorldPoint(*/new Vector2(safeAreaDeadzone.size.x * Screen.width, safeAreaDeadzone.size.y * Screen.height);

        //Debug.Log("DSA " + size.ToString());
        Debug.Log(Input.mousePosition.ToString());

        Gizmos.color = new Color(0, 1, 0, .5f);
        //Gizmos.DrawCube(safeAreaDeadzone.center, safeAreaDeadzone.size);
        //Rect relative = new Rect()
        Gizmos.DrawCube(position, size);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) == true)
        {
            gameCamera.FollowWithSafeZone(myCatapult.currentProjectile.GetComponent<Collider2D>().bounds, safeAreaSize);
        }

        gameCamera.UpdateSafeZone(myCatapult.currentProjectile.GetComponent<Collider2D>().bounds);
    }
}
