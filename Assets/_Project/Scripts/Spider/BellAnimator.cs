using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class BellAnimator : MonoBehaviour
{
    [SerializeField] float maxAngle = 5;
    [SerializeField] float windSpeed = 1f;
    [SerializeField] float windForce = 1f;

    Vector3 initUp;
    float noiseOffset;

    private void Start ()
    {
        initUp = transform.parent.InverseTransformDirection(transform.up);
        noiseOffset = UnityEngine.Random.value * 100;
    }

    public void ManualUpdate()
    {
        Vector3 currentUp = transform.parent.InverseTransformDirection(transform.up);
        float2 windOffset = new float2(
            Mathf.PerlinNoise1D(Time.time * windSpeed + noiseOffset),
            Mathf.PerlinNoise1D(Time.time * windSpeed - noiseOffset)) * windForce;
        Vector3 desireUp = transform.parent.InverseTransformDirection((Vector3.up + new Vector3(windOffset.x, 0f, windOffset.y)).normalized);

        float angle = Vector3.Angle(initUp, desireUp);
        if(angle != 0f)
        {
            float percent = Mathf.Min(angle, maxAngle) / angle;
            Vector3 allowedUp = Vector3.Slerp(initUp, desireUp, percent);

            transform.rotation *= Quaternion.FromToRotation(currentUp, allowedUp);
        }
    }
}
