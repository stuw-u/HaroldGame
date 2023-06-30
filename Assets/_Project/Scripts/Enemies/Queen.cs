using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class Queen : Enemy
{
    [Header("Queen Settings")]
    [SerializeField] Animator animator;
    [SerializeField] Transform rotAnchor;
    [SerializeField] float smoothRot = 0.5f;
    [SerializeField] float playerMaxDist = 4f;
    [SerializeField] float stopDistance = 4f;

    bool wasTargettingPlayer = false;
    Vector3 dir = Vector3.forward;
    Vector3 lastPos = Vector3.zero;
    int triggerId = 0;

    protected override void OnRestore ()
    {
        triggerId = Animator.StringToHash("Hit");
    }

    protected override void OnDeath ()
    {
        AudioManager.Play(AudioClipName.AloesDeath, transform.position);
    }

    
    protected override void OnUpdate ()
    {
        if (Time.deltaTime == 0f) return;

        Vector3 estimatedVel = transform.position - lastPos;
        estimatedVel /= Time.deltaTime;

        


        var isTargettingPlayer = false;
        Vector3 diff = GameManager.Player.transform.position - transform.position;
        if (diff.sqrMagnitude < playerMaxDist * playerMaxDist && agent.isOnNavMesh)
        {
            isTargettingPlayer = true;
            estimatedVel.y = 0f;
            estimatedVel = diff.normalized;

            animator.SetTrigger(triggerId);
        }

        if (estimatedVel != Vector3.zero)
        {
            Vector3 targetDir = estimatedVel;
            targetDir.y *= 0.1f;
            targetDir.Normalize();
            var blend = 1f - math.pow(1f - smoothRot, Time.deltaTime * 60);
            dir = Vector3.Slerp(dir, targetDir, blend);

            rotAnchor.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        if (isTargettingPlayer && agent.isOnNavMesh)
        {
            agent.SetDestination(GameManager.Player.transform.position);
            agent.stoppingDistance = stopDistance;
        }
        if(wasTargettingPlayer && !isTargettingPlayer && agent.isOnNavMesh)
        {
            agent.SetDestination(lastNextPoint);
            agent.stoppingDistance = 0f;
        }

        wasTargettingPlayer = isTargettingPlayer;
        lastPos = transform.position;

        rotAnchor.localPosition = Vector3.up * (realY - transform.position.y);
    }

    public void OnAnimationPlay ()
    {
        AudioManager.Play(AudioClipName.MineDrop, transform.position);
    }
}
