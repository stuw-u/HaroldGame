using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AI;

public class Pini : Enemy
{
    [Header("Pini Settings")]
    [SerializeField] GameObject eyes;
    [SerializeField] ParticleSystem leaves;
    [SerializeField] SpiderProceduralAnimation spider;
    [SerializeField] Transform rotAnchor;
    [SerializeField] float maxHeight;
    [SerializeField] Transform topTrunc;
    [SerializeField] Transform topTruncLoweredCopy;
    [SerializeField] float baseHeight = 2f;
    [SerializeField] float smoothRot = 0.5f;
    [SerializeField] float loweredSmooth = 0.5f;
    [SerializeField] BellAnimator[] bellAnimators;

    Vector3 topTruncDefaultPos;
    Quaternion topTruncDefaultRot;
    Vector3 topTruncDefaultScale;
    Vector3 topTruncLoweredPos;
    Quaternion topTruncLoweredRot;
    Vector3 topTruncLoweredScale;
    float loweredValueSmooth;
    float loweredValueVel;

    protected override void OnInit ()
    {
        topTruncDefaultPos = topTrunc.localPosition;
        topTruncDefaultRot = topTrunc.localRotation;
        topTruncDefaultScale = topTrunc.localScale;
        topTruncLoweredPos = topTruncLoweredCopy.localPosition;
        topTruncLoweredRot = topTruncLoweredCopy.localRotation;
        topTruncLoweredScale = topTruncLoweredCopy.localScale;
    }

    protected override void OnRestore ()
    {
        eyes.SetActive(true);
        leaves.Play();
        dir = transform.forward;
    }

    protected override void OnDeath ()
    {
        eyes.SetActive(false);
        leaves.Stop();
        AudioManager.Play(AudioClipName.PineTreeDeath, transform.position);
    }

    Vector3 dir = Vector3.forward;
    Vector3 lastPos = Vector3.zero;
    protected override void OnUpdate ()
    {
        if(Time.deltaTime == 0f) { return; }

        foreach(var bellAnimator in bellAnimators)
        {
            bellAnimator.ManualUpdate();
        }

        spider.ManualUpdate();

        Vector3 estimatedVel = transform.position - lastPos;
        estimatedVel /= Time.deltaTime;

        if (estimatedVel != Vector3.zero)
        {
            Vector3 targetDir = -estimatedVel;
            targetDir.y *= 0.5f;
            targetDir.Normalize();
            var blend = 1f - math.pow(1f - smoothRot, Time.deltaTime * 60);
            dir = Vector3.Slerp(dir, targetDir, blend);

            rotAnchor.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        bool cast1 = Physics.Raycast(transform.position + transform.forward * 4f, Vector3.up, out var hit1, maxHeight + baseHeight, 1 << 7);
        float minDistance = cast1 ? hit1.distance : 1000f;

        if(loweredValueSmooth != 0)
        {
            bool cast2 = Physics.Raycast(transform.position - transform.forward * 4f, Vector3.up, out var hit2, maxHeight + baseHeight, 1 << 7);
            minDistance = Mathf.Min(minDistance, cast2 ? hit2.distance : 1000f);
        }
        var loweredValue = Mathf.Clamp01(1f - (Mathf.Max(0f, minDistance - baseHeight) / (maxHeight - baseHeight)));

        loweredValueSmooth = Mathf.SmoothDamp(loweredValueSmooth, loweredValue, ref loweredValueVel, loweredSmooth);
        topTrunc.localPosition = Vector3.Lerp(topTruncDefaultPos, topTruncLoweredPos, loweredValueSmooth);
        topTrunc.localRotation = Quaternion.Slerp(topTruncDefaultRot, topTruncLoweredRot, loweredValueSmooth);
        topTrunc.localScale = Vector3.Lerp(topTruncDefaultScale, topTruncLoweredScale, loweredValueSmooth);

        lastPos = transform.position;

        rotAnchor.localPosition = Vector3.up * (realY - transform.position.y);
    }

    protected override void OnFixedUpdate ()
    {
        spider.ManualFixedUpdate();
    }
}
