using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProceduralAnimation : MonoBehaviour
{
    [Header("References")]
    public Transform[] legRaycastOrigin;
    public Transform[] legTargets;
    public Transform body;
    public Transform anchor;
    [SerializeField] AudioSource[] sources;
    [SerializeField] AudioClip[] sounds;

    [Header("Parameters")]
    public float stepSize = 1f;
    public float legMovingSpeed = 6f;
    public float velSmooth = 1;
    public float orientationSmooth = 0.1f;
    public float stepHeight = 0.1f;
    public bool bodyOrientation = true;
    public float velocityMultiplier = 15f;
    public float legSpeedVelMul = 0.1f;
    public float raycastRange = 1f;




    private float[] legMovingValue;
    private Vector3[] lastSafeLegPositions;
    private Vector3[] nextSafeLegPositions;
    private Vector3[] legNormal;
    private Vector3[] desiredPositions;

    private Vector3 lastBodyUp;
    private int nbLegs;
    
    private Vector3 velocity;
    private Vector3 velocityDamp;
    private Vector3 dampV;
    private Vector3 lastBodyPos;


    
    
    void Start()
    {
        // Caching variables
        lastBodyUp = transform.up;
        nbLegs = legTargets.Length;

        lastSafeLegPositions = new Vector3[nbLegs];
        nextSafeLegPositions = new Vector3[nbLegs];

        desiredPositions = new Vector3[nbLegs];
        legNormal = new Vector3[nbLegs];
        legMovingValue = new float[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            legNormal[i] = transform.up;
            legMovingValue[i] = 0;
        }
        lastBodyPos = transform.position;
    }

    public void ManualUpdate ()
    {

        // Orient body toward estimated normal
        // Normal is estimated from 3 leg positions
        if (nbLegs > 3 && bodyOrientation)
        {
            Vector3 avgNormal = default;
            for (int i = 0; i < nbLegs; ++i)
            {
                avgNormal += legNormal[i];
            }
            avgNormal.Normalize();
            var blend = 1f - Mathf.Pow(1f - orientationSmooth, Time.deltaTime * 60);
            Vector3 up = Vector3.Lerp(lastBodyUp, avgNormal, blend);
            body.up = up;
            lastBodyUp = up;
        }

        float legSpeedMul = 1f + velocityDamp.magnitude * legSpeedVelMul;

        // Perform leg animation (or reset position)
        for (int i = 0; i < nbLegs; ++i)
        {
            float value = legMovingValue[i];
            if (value > 0f)
            {

                value = Mathf.Clamp01(value - Time.deltaTime * legMovingSpeed * legSpeedMul);
                Vector3 upOffset = LegYAnimation(value) * stepHeight * transform.up;
                var next = nextSafeLegPositions[i];
                if(nextSafeLegPositions[i] == Vector3.zero)
                {
                    nextSafeLegPositions[i] = legRaycastOrigin[i].position - Vector3.up * 0.5f;
                }
                legTargets[i].position = Vector3.Lerp(nextSafeLegPositions[i], lastSafeLegPositions[i], value) + upOffset;

                if(value == 0f)
                {
                    lastSafeLegPositions[i] = nextSafeLegPositions[i];
                }
            }
            else
            {
                legTargets[i].position = lastSafeLegPositions[i];
            }
            legMovingValue[i] = value;
        }
    }


    public void ManualFixedUpdate()
    {
        // Calculate damped velocity
        velocity = transform.position - lastBodyPos;
        velocityDamp = Vector3.SmoothDamp(velocityDamp, velocity, ref dampV, velSmooth);
        lastBodyPos = transform.position;

        Debug.DrawLine(transform.position, transform.position + velocityDamp);

        // Find which leg to move, if any.
        int indexToMove = -1;
        float largestDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            int pairedLeg = (i / 2) + (i % 2 == 0 ? 1 : 0);

            desiredPositions[i] = legRaycastOrigin[i].position;
            desiredPositions[i] += Vector3.ClampMagnitude(velocity * velocityMultiplier, stepSize);
            (var didHit, var targetPos, var targetNormal) = RaycastFeet(desiredPositions[i], raycastRange, anchor.up);
            desiredPositions[i] = targetPos;

            if (!didHit)
            {
                nextSafeLegPositions[i] = Vector3.zero;
                legNormal[i] = targetNormal;
                legMovingValue[i] = 0.01f;
            }
            else if (!IsLegMoving(i) && !IsLegMoving(pairedLeg))
            {
                float dist = Vector3.Distance(lastSafeLegPositions[i], desiredPositions[i]);

                if (dist > stepSize)
                {
                    largestDistance = dist;
                    indexToMove = i;
                }
            }
        }

        if (indexToMove != -1)
        {
            (var didHit, var targetPos, var targetNormal) = RaycastFeet(desiredPositions[indexToMove], raycastRange, anchor.up);
            if(didHit)
            {
                if(Random.value < 0.3f)
                {
                sources[indexToMove].clip = sounds[Random.Range(0, sources.Length)];
                sources[indexToMove].pitch = Random.Range(0.8f, 1.2f);
                sources[indexToMove].Play();

                }
                nextSafeLegPositions[indexToMove] = targetPos;
                legNormal[indexToMove] = targetNormal;
                legMovingValue[indexToMove] = 1f;
            }
        }
    }

    private bool IsLegMoving (int index) => legMovingValue[index] != 0;

    (bool didHit, Vector3 point, Vector3 normal) RaycastFeet (Vector3 point, float offsetY, Vector3 up)
    {
        if (Physics.Raycast(point + offsetY * up, -up, out var hit, 2f * offsetY))
            return (true, hit.point, hit.normal);
        else
            return (false, point, up);
    }

    float LegYAnimation (float value)
    {
        return Mathf.Clamp01(1-4*((value-0.5f) * (value - 0.5f)));
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastSafeLegPositions[i], 0.05f);
            Gizmos.DrawSphere(nextSafeLegPositions[i], 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(legRaycastOrigin[i].position, stepSize);
        }
    }
}
