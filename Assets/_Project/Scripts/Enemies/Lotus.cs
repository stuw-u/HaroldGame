using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class Lotus : Enemy
{
    [Header("Lotus Settings")]
    [SerializeField] Animator animator;
    [SerializeField] Transform rotAnchor;
    [SerializeField] float playerSteerDist = 4f;
    [SerializeField] float playerHitDist = 4f;
    [SerializeField] float playerSteerSpeed = 4f;
    [SerializeField] float spinSpeed = 45f;

    Vector3 vel;

    protected override void OnRestore ()
    {
        /*animation.Play();
        animation["Snap"].speed = 1;*/
    }

    protected override void OnDeath ()
    {
        AudioManager.Play(AudioClipName.LotusDeath, transform.position);
    }

    Vector3 lastPos = Vector3.zero;
    Vector3 smoothUp = Vector3.zero;
    float spin = 0f;
    Vector3 velvel;
    bool playEffect;
    protected override void OnUpdate ()
    {
        if (Time.deltaTime == 0f) return;

        Vector3 estimatedVel = transform.position - lastPos;
        estimatedVel /= Time.deltaTime;

        Vector3 up = Vector3.up;
        float offsetY = 0.25f;
        if(Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out var hitFront, 4f, 1 << 7))
        {
            up = hitFront.normal;
        }
        smoothUp = Vector3.RotateTowards(smoothUp, up, Time.deltaTime * math.radians(90), 1f);
        offsetY += 1f-Vector3.Dot(smoothUp, Vector3.up);
        rotAnchor.localPosition = Vector3.up * (offsetY + (realY - transform.position.y));



        Vector3 diff = GameManager.Player.transform.position - transform.position;
        diff.y = 0;
        if (diff.sqrMagnitude < playerSteerDist * playerSteerDist && diff.sqrMagnitude != 0f)
        {
            if (!playEffect)
            {
                AudioManager.Play(AudioClipName.LotusSurge, transform.position, false);
                playEffect = true;
            }
            
            float value = 1f - (diff.magnitude / playerSteerDist);
            if(diff.sqrMagnitude > playerHitDist * playerHitDist)
            {
                value *= Mathf.Clamp01(Vector3.Dot(estimatedVel.normalized, diff.normalized));
            }
            else
            {
                value *= 3f;
            }
            //diff = Vector3.ClampMagnitude(diff, playerSteerSpeed * Time.deltaTime);
            vel += diff * value * playerSteerSpeed * Time.deltaTime;
        }
        else
        {
            playEffect = false;
        }
        vel = Vector3.SmoothDamp(vel, Vector3.zero, ref velvel, 1f);
        if(agent.isOnNavMesh && !IsDead)
        {
            agent.Move(vel * Time.deltaTime);
        }



        spin += Time.deltaTime * spinSpeed;
        rotAnchor.rotation = Quaternion.FromToRotation(Vector3.up, smoothUp) * Quaternion.Euler(0f, spin, 0f);

        lastPos = transform.position;
    }

    protected override void OnHitPlayer ()
    {
        animator.SetTrigger("Hit");
        agent.SetDestination(transform.position);
        vel = Vector3.zero;
    }
}
