using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
