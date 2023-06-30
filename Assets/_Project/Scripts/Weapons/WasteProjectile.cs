using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WasteProjectile : MonoBehaviour
{
    [SerializeField] int damage = 40;
    [SerializeField] float arealLifetime = 2f;
    [SerializeField] float groundLifetime = 5f;
    [SerializeField] float hitSphereRadius = 0.75f;

    private Rigidbody body;
    private Collider coll;
    private DecalProjector decal;
    private bool hitGround = false;
    private float time = 0f;
    private Vector3 initSize;

    private Collider[] collCache;
    private void Start ()
    {
        collCache = new Collider[4];

        if (body == null)
        {
            body = GetComponent<Rigidbody>();
            coll = GetComponent<Collider>();
            decal = GetComponent<DecalProjector>();
            decal.fadeFactor = 0f;
            initSize = decal.size;
        }

    }

    public void Shoot (Vector3 initPos, Vector3 vel)
    {
        Start();

        transform.position = initPos;
        body.velocity = vel;
    }

    private void FixedUpdate ()
    {
        if(!hitGround)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, hitSphereRadius, collCache, ~0, QueryTriggerInteraction.Collide);
            for(int i = 0; i < hitCount; i++)
            {
                if(collCache[i].TryGetComponent<Damagable>(out var component))
                {
                    component.ApplyHit(damage);
                    time = arealLifetime;
                }
            }

            if (body.velocity.sqrMagnitude > 0.1f)
            {
                transform.forward = body.velocity.normalized;
            }

            if(time >= arealLifetime)
            {
                Destroy(gameObject);
            }
        } 
        else
        {
            float fadeValue = (time / groundLifetime);
            fadeValue *= fadeValue;
            fadeValue = 1-fadeValue;
            float sizeFactor = Mathf.Lerp(1f, 0.5f, 1f-fadeValue);

            decal.fadeFactor = fadeValue;
            decal.size = new Vector3(initSize.x * sizeFactor, initSize.y * sizeFactor, initSize.z);

            if(time > groundLifetime)
            {
                Destroy(gameObject);
            }
        }


        
        time += Time.deltaTime;
    }

    private void OnCollisionEnter (Collision collision)
    {
        time = 0f;
        hitGround = true;
        body.isKinematic = true;
        coll.enabled = false;
        decal.fadeFactor = 1f;

        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2f))
        {
            transform.forward = (transform.forward + -hit.normal * 3).normalized;
            
            GroundSystem.ApplyBurnt(hit);
        }
    }
}
