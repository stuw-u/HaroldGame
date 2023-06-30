using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamager : MonoBehaviour
{
    [SerializeField] float impulse = 5f;
    [SerializeField] bool needToBeOnGround;
    [SerializeField] Enemy source;

    private void OnTriggerStay (Collider other)
    {
        if (source == null) return;
        if (source.IsDead) return;

        if(other.gameObject.layer == 9)
        {
            if(other.TryGetComponent<PlayerController>(out var component))
            {
                if (!component.IsGrounded && needToBeOnGround) return;
                component.Kill();
                component.ApplyImpulse(transform.forward * impulse);
                source.TriggerHitPlayer();
            }
        }
    }
}
