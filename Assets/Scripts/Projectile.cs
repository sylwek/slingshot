using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float stoppedThreshold = 1f;

    private GameManager gameManager = null;
    private bool collisionOccured = false;
    private bool countdownRequested = false;
    private Queue<Vector2> cumulativeVelocity = null;
    private int VelocityDesiredSampleCount = 30;

    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        cumulativeVelocity = new Queue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!countdownRequested && IsActive())
        {
            if (!countdownRequested && !IsInCameraView())
            {
                Debug.Log("Projectile out of scene. Starting countdown for next shoot.");
                RequestCountdown();
                //Destroy(this.gameObject);
            }

            cumulativeVelocity.Enqueue(GetComponent<Rigidbody2D>().velocity);
            if (cumulativeVelocity.Count > VelocityDesiredSampleCount)
            {
                cumulativeVelocity.Dequeue();
                if (GetCumulativeVelocity().magnitude < stoppedThreshold)
                    RequestCountdown();
            }
        }

        //if(countdownRequested && IsActive() && !IsInCameraView())
        //    Destroy(this.gameObject);

    }

    //
    public void Activate(bool bActive)
    {
        gameObject.GetComponent<Rigidbody2D>().isKinematic = !bActive;
    }

    //
    public bool IsActive()
    {
        return !gameObject.GetComponent<Rigidbody2D>().isKinematic;
    }

    //
    public void ApplyForce(Vector2 force)
    {
        Debug.Log("Launching projectile with force: " + force.ToString());
        GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

    private bool IsInCameraView()
    {
        Vector2 viewportPos = gameManager.gameCamera.WorldToViewportPoint(transform.position);
        bool result = Vector2.Max(viewportPos, Vector2.zero) == viewportPos && Vector2.Min(viewportPos, Vector2.one) == viewportPos;
        return result;
    }

    private void RequestCountdown()
    {
        gameManager.StartCountdownToInitNextShoot();
        countdownRequested = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collisionOccured
            && collision.otherCollider.gameObject.GetComponent<Projectile>() == null
            && collision.otherCollider.gameObject != this.gameObject
            && gameManager != null)
        {
            gameManager.OnFirstProjectileCollision();
        }

        collisionOccured = true;
    }

    private Vector2 GetCumulativeVelocity()
    {
        var result = Vector2.zero;
        foreach (var velocity in cumulativeVelocity)
            result += velocity;

        //Debug.Log("CumulativeVelocity: " + result.ToString());
        return result;
    }
}
