using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Weapon
{
    [SerializeField] LineRenderer line;
    [SerializeField] Transform shootTip;
    [SerializeField] ParticleSystem recharged;
    [SerializeField] float cooldownRefill = 1;
    [SerializeField] AudioSource fireSoure;

    private float juice;
    private RaycastHit[] hitCache; 

    private void Awake ()
    {
        hitCache = new RaycastHit[8];
    }
    public override void OnUpdate (bool isUsed)
    {
        line.SetPosition(0, shootTip.position);
        var dist = 100f;
        int hits = Physics.RaycastNonAlloc(shootTip.position, shootTip.forward, hitCache, 100f, 1 << 7 | 1 << 8, QueryTriggerInteraction.Collide);
        if (hits > 0)
        {
            dist = hitCache[0].distance;
        }
        var point = shootTip.position + shootTip.forward * dist;
        line.SetPosition(1, point);


        if (juice > 0f)
        {
            juice = Mathf.Clamp01(juice - cooldownRefill * Time.deltaTime);
            if (juice == 0f)
            {
                recharged.Play();
            }
        }

        if (isUsed && !fireSoure.isPlaying) fireSoure.Play();
        if (!isUsed && fireSoure.isPlaying) fireSoure.Stop();
        fireSoure.volume = juice * 0.7f;

        if ((isUsed && juice == 0f) || (juice > 0.9f))
        {
            

            for(int i = 0; i < hits; i++)
            {
                if (hitCache[i].collider.TryGetComponent<Damagable>(out var component))
                {
                    component.ApplyHit(1000);
                    AudioManager.Play(AudioClipName.BurnTick, hitCache[i].collider.transform.position);
                }
            }
            
            if(juice == 0)
            {
                juice = 1f;
            }
        }
        line.enabled = Random.value > (1f - (juice * juice));
    }
}
