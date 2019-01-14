using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotate : MonoBehaviour
{
    public Vector2 shootPower = Vector2.one;
    public int dotsCount = 20;
    public float dotSeparation = 1f;                 //The space between the points representing the trajectory
    public float dotShift = 3f;						//How far the first dot is from the "ball"
    private float baseAngle = 0.0f;
    private Vector2 shotForce;					//How much velocity will be applied to the ball
    private bool dragging = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        Debug.Log("MouseDown");
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        pos = Input.mousePosition - pos;
        baseAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        baseAngle -= Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
    }

    void OnMouseDrag()
    {
        Debug.Log("OnMouseDrag");
        dragging = true;
        //Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        //pos = Input.mousePosition - pos;
        //float ang = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg - baseAngle;
        //transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);

        var fingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);    //The position of your finger/cursor is found
        fingerPos.z = 0;													//The z position is set to 0

        var ballFingerDiff = transform.position - fingerPos;								//The distance between the finger/cursor and the "ball" is found
        shotForce = new Vector2(ballFingerDiff.x * shootPower.x, ballFingerDiff.y * shootPower.y);    //The velocity of the shot is found
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        for (int k = 0; k < dotsCount; k++)
        {                           //Each point of the trajectory will be given its position
            var x1 = transform.position.x + shotForce.x * Time.fixedDeltaTime * (dotSeparation * k + dotShift);    //X position for each point is found
            var y1 = transform.position.y + shotForce.y * Time.fixedDeltaTime * (dotSeparation * k + dotShift) - (-Physics2D.gravity.y / 2f * Time.fixedDeltaTime * Time.fixedDeltaTime * (dotSeparation * k + dotShift) * (dotSeparation * k + dotShift));    //Y position for each point is found
            //dots[k].transform.position = new Vector3(x1, y1, dots[k].transform.position.z); //Position is applied to each point
            Gizmos.DrawSphere(new Vector3(x1, y1, Camera.main.nearClipPlane), .1f);
            //Debug.Log("Dot #" + k.ToString() + " Pos: " + new Vector2(x1, y1).ToString());
        }
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    //TrajectoryData CalculateTrajectoryData()
    //{
    //    float displacementY = target.position.y - ball.position.y;
    //    Vector3 displacementXZ = new Vector3(target.position.x - ball.position.x, 0, target.position.z - ball.position.z);
    //    float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
    //    Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
    //    Vector3 velocityXZ = displacementXZ / time;

    //    return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravity), time);
    //}

}

//struct TrajectoryData
//{
//    public readonly Vector2 initialVelocity;
//    public readonly float flightTime;

//    public TrajectoryData(Vector2 initialVelocity, float flightTime)
//    {
//        this.initialVelocity = initialVelocity;
//        this.flightTime = flightTime;
//    }
//}
