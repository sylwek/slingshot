using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    private float Duration = 0.2f;
    private float Power = 1.0f;
    private bool Shaking = false;
    private Vector3 InitialPosition = Vector3.zero;

    // Use this for initialization
    void Start()
    {

    }

    public void Shake(float duration, float power)
    {
        Duration = duration;
        Power = power;
        InitialPosition = transform.position;
        Shaking = true;
    }

    // Update is called once per frame
    void Update()
    {
        //coroutines?
        if(Shaking)
        {
            var randomVec3 = Random.insideUnitSphere;
            randomVec3.z = 0;
            transform.position = InitialPosition + randomVec3 * Power;
            Duration -= Time.deltaTime;
        }

        if(Duration <= 0f)
        {
            transform.position = InitialPosition;
            Shaking = false;
        }
    }
}
