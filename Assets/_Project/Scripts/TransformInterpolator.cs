using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How to use TransformInterpolator properly:
/// 0. Make sure the gameobject executes its mechanics (transform-manipulations)
/// in FixedUpdate().
/// 1. Make sure VSYNC is enabled.
/// 2. Set the execution order for this script BEFORE all the other scripts
/// that execute mechanics.
/// 3. Attach (and enable) this component to every gameobject that you want to interpolate
/// (including the camera).
/// </summary>

[DefaultExecutionOrder(-1)]
public class TransformInterpolator : MonoBehaviour
{
    private struct TransformData
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }

    //Init prevTransformData to interpolate from the correct state in the first frame the interpolation becomes active. This can occur when the object is spawned/instantiated.
    void OnEnable ()
    {
        prevTransformData.position = transform.localPosition;
        prevTransformData.rotation = transform.localRotation;
        prevTransformData.scale    = transform.localScale;
        isTransformInterpolated    = false;
    }

    void FixedUpdate ()
    {
        //Reset transform to its supposed current state just once after each Update/Drawing.
        if (isTransformInterpolated)
        {
            transform.localPosition = transformData.position;
            transform.localRotation = transformData.rotation;
            transform.localScale    = transformData.scale;

            isTransformInterpolated = false;
        }

        //Cache current transform state as previous
        //(becomes "previous" by the next transform-manipulation
        //in FixedUpdate() of another component).
        prevTransformData.position = transform.localPosition;
        prevTransformData.rotation = transform.localRotation;
        prevTransformData.scale    = transform.localScale;
    }

    void LateUpdate ()   //Interpolate in Update() or LateUpdate().
    {
        //Cache the updated transform so that it can be restored in
        //FixedUpdate() after drawing.
        if (!isTransformInterpolated)
        {
            transformData.position = transform.localPosition;
            transformData.rotation = transform.localRotation;
            transformData.scale    = transform.localScale;

            //This promise matches the execution that follows after that.
            isTransformInterpolated = true;
        }

        //(Time.time - Time.fixedTime) is the "unprocessed" time according to documentation.
        float interpolationAlpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        //Interpolate transform:
        transform.localPosition = Vector3.Lerp(prevTransformData.position,
        transformData.position, interpolationAlpha);
        transform.localRotation = Quaternion.Slerp(prevTransformData.rotation,
        transformData.rotation, interpolationAlpha);
        transform.localScale = Vector3.Lerp(prevTransformData.scale,
        transformData.scale, interpolationAlpha);
    }

    private TransformData transformData;
    private TransformData prevTransformData;
    private bool isTransformInterpolated;
}