using UnityEngine;
using System.Collections;

// mkapusta 18.03.2016
public class ParallaxDephtInfluence : MonoBehaviour
{
    // parameters
    public float paralaxDepth = 0.0f;

    // members
    private Vector3 v2ObjectSize;


    //
    public void Start()
    {
        v2ObjectSize = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size;
        v2ObjectSize.x *= gameObject.transform.localScale.x;
        v2ObjectSize.y *= gameObject.transform.localScale.y;
    }

    //
    public void UpdatePosition(Rect camRect, Vector3 v3CameraMoveVector)
    {
        gameObject.transform.localPosition += v3CameraMoveVector * (-paralaxDepth);

        float fObjectRightBoundPos = gameObject.transform.position.x + v2ObjectSize.x / 2.0f;
        float fCameraLeftBoundPos = camRect.position.x;

        if (fObjectRightBoundPos < fCameraLeftBoundPos)
            gameObject.transform.localPosition += new Vector3(4.0f * v2ObjectSize.x, 0.0f, 0.0f);

        float fObjectLeftBoundPos = gameObject.transform.position.x - v2ObjectSize.x / 2.0f;
        float fCameraRightBoundPos = camRect.position.x + camRect.width;

        if (fObjectLeftBoundPos > fCameraRightBoundPos)
            gameObject.transform.localPosition -= new Vector3(4.0f * v2ObjectSize.x, 0.0f, 0.0f);
    }
}
