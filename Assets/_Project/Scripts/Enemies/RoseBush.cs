using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class RoseBush : Enemy
{
    [Header("Rose Settings")]
    [SerializeField] Animation animation;
    [SerializeField] Transform rotAnchor;
    [SerializeField] float smoothRot = 0.5f;

    protected override void OnRestore ()
    {
        animation.Play();
        animation["RoseWalk"].speed = 2;
    }

    protected override void OnDeath ()
    {
        AudioManager.Play(AudioClipName.RoseDeath, transform.position);
    }

    Vector3 dir = Vector3.forward;
    Vector3 lastPos = Vector3.zero;
    protected override void OnUpdate ()
    {
        if (Time.deltaTime == 0f) return;

        Vector3 estimatedVel = transform.position - lastPos;
        estimatedVel /= Time.deltaTime;

        if (estimatedVel != Vector3.zero)
        {
            Vector3 targetDir = estimatedVel;
            targetDir.y *= 0.1f;
            targetDir.Normalize();
            var blend = 1f - math.pow(1f - smoothRot, Time.deltaTime * 60);
            dir = Vector3.Slerp(dir, targetDir, blend);

            rotAnchor.rotation = Quaternion.LookRotation(dir, Vector3.up);
            rotAnchor.localPosition = Vector3.up * (realY - transform.position.y);
        }

        lastPos = transform.position;
    }
}
