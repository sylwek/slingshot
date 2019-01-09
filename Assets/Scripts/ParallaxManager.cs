using UnityEngine;
using System.Collections;

public class ParallaxManager : MonoBehaviour
{
    // members
    private ParallaxDephtInfluence[] parallaxObjects;
    private Vector3 _v3PrevCamPos = Vector3.zero;
    private float _fPrevCamOrtho = 0.0f;

    //
    private const float fMinMoveDelta = 0.0001f;
    private const float fMinOrthoChange = 0.0001f;

    GameManager gameManager = null;


    //
    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        parallaxObjects = gameObject.GetComponentsInChildren<ParallaxDephtInfluence>();

        _v3PrevCamPos = gameManager.gameCamera.transform.position;
        _fPrevCamOrtho = gameManager.gameCamera.GetComponent<Camera>().orthographicSize;
    }

    //
    void Update()
    {
        Vector3 v3CamMoveVector = gameManager.gameCamera.transform.position - _v3PrevCamPos;
        float fCamOrthoChange = gameManager.gameCamera.GetComponent<Camera>().orthographicSize - _fPrevCamOrtho;

        if ((Mathf.Abs(v3CamMoveVector.x) > fMinMoveDelta) || (Mathf.Abs(fCamOrthoChange) > fMinOrthoChange))
        {
            Vector3 llPoint = gameManager.gameCamera.GetComponent<Camera>().ScreenToWorldPoint(Vector3.zero);
            Vector3 urPoint = gameManager.gameCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(gameManager.gameCamera.GetComponent<Camera>().pixelWidth, gameManager.gameCamera.GetComponent<Camera>().pixelHeight, 0.0f));
            Rect CamRect = new Rect(llPoint.x, llPoint.y, urPoint.x - llPoint.x, urPoint.y - llPoint.y);

            v3CamMoveVector.y = 0.0f;

            for (int i = 0; i < parallaxObjects.Length; i++)
                parallaxObjects[i].UpdatePosition(CamRect, v3CamMoveVector);

            _v3PrevCamPos = gameManager.gameCamera.transform.position;
        }
    }
}
