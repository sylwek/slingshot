using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    bool CollisionOccured = false;
    bool CountdownRequested = false;
    GameManager gameManager = null;
    Queue<Vector2> cumulativeVelocity = null;
    int VelocityDesiredSampleCount = 30;

    // Use this for initialization
    void Start()
    {
        gameManager = Object.FindObjectOfType<GameManager>();
        cumulativeVelocity = new Queue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsActive() && !CountdownRequested)
        {
            cumulativeVelocity.Enqueue(GetComponent<Rigidbody2D>().velocity);
            if (cumulativeVelocity.Count > VelocityDesiredSampleCount)
            {
                cumulativeVelocity.Dequeue();
                if (GetCumulativeVelocity().magnitude < 0.1f)
                {
                    gameManager.StartCountdownToNextShoot();
                    CountdownRequested = true;
                }
            }

        }
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
    public void ApplyVelocity(Vector2 shootVelocity)
    {
        GetComponent<Rigidbody2D>().velocity = shootVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!CollisionOccured && gameManager != null)
            gameManager.OnFirstProjectileCollision();

        CollisionOccured = true;
    }

    private Vector2 GetCumulativeVelocity()
    {
        var result = Vector2.zero;
        foreach (var velocity in cumulativeVelocity)
            result += velocity;

        Debug.Log("CumulativeVelocity: " + result.ToString());
        return result;
    }
}
