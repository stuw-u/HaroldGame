using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineProjectile : MonoBehaviour
{
    [SerializeField] ParticleSystem boom;
    [SerializeField] float blowRadius = 4f;
    static Collider[] colls = new Collider[8];

    private void OnTriggerEnter (Collider other)
    {
        if(other.gameObject.layer == 8)
        {
            Ignite();

            int count = Physics.OverlapSphereNonAlloc(transform.position, blowRadius, colls, 1 << 8, QueryTriggerInteraction.Collide);
            for(int i = 0; i < count; i++)
            {
                if(colls[i].gameObject.TryGetComponent<Damagable>(out var component))
                {
                    component.ApplyHit(1000);
                }
            }
        }
    }

    public void Ignite ()
    {
        GetComponent<Collider>().enabled = false;
        boom.Play();
        boom.transform.parent = null;
        Destroy(gameObject);
        Destroy(boom.gameObject, 1f);
        AudioManager.Play(AudioClipName.MineBlowUp, transform.position);
    }

    private void Update ()
    {
        if(transform.position.y < -5 || !GameManager.IsEnemySpawningAllowed)
        {
            Destroy(gameObject);
        }
    }
}
