using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : Weapon
{
    [SerializeField] float raycastLength = 4;
    [SerializeField] Transform tip;
    [SerializeField] Transform particleTr;
    [SerializeField] Transform burnA;
    [SerializeField] Transform burnB;
    [SerializeField] float hitRadius = 0.5f;
    [SerializeField] new Light light;
    [SerializeField] ParticleSystem particles;
    [SerializeField] float tipSmoothing = 0.5f;
    [SerializeField] float lightFadeSpeed = 1f;
    [SerializeField] float tickInterval = 0.1f;
    [SerializeField] AudioSource fireSource;

    private float lightIntensity;
    private float lightMaxIntensity;
    Vector3 fwd = default;
    private float timer;
    private Collider[] cachedHits;

    private void Awake ()
    {
        cachedHits = new Collider[8];

        lightMaxIntensity = light.intensity;
        particles.Stop(true);
    }

    public override void OnBeginUse ()
    {
        fwd = tip.forward;
        particles.Play(true);
    }

    
    public override void OnUse ()
    {
        timer += Time.deltaTime;
        if(timer > tickInterval && fwd.sqrMagnitude > 0.1f)
        {
            int hits = Physics.OverlapCapsuleNonAlloc(burnA.position, burnB.position, hitRadius, cachedHits, 1<<8, QueryTriggerInteraction.Collide);
            for(int i = 0; i < hits; i++)
            {
                if(cachedHits[i].TryGetComponent<Flammable>(out var component))
                {
                    AudioManager.Play(AudioClipName.BurnTick, cachedHits[i].transform.position);
                    component.ApplyFireHit();
                }
            }

            timer = Mathf.Repeat(timer, tickInterval);
        }
    }

    public override void OnUpdate (bool isUsing)
    {
        lightIntensity = Mathf.Clamp01(lightIntensity + (isUsing ? 1 : -1) * lightFadeSpeed * Time.deltaTime);
        light.intensity = lightIntensity * lightMaxIntensity;

        // Disable/enable light if intensity is 0/is not 0
        if((lightIntensity != 0) != light.gameObject.activeSelf) light.gameObject.SetActive(lightIntensity != 0);


        // Flame direction
        Vector3 targetFwd = tip.forward;
        if (Physics.Raycast(tip.position, targetFwd, out RaycastHit hit, raycastLength))
        {
            var dirBlend = hit.distance / raycastLength;
            Vector3 reflectedFwd = Vector3.Slerp(Vector3.Reflect(targetFwd, hit.normal), Vector3.ProjectOnPlane(targetFwd, hit.normal), 0.75f);
            targetFwd = Vector3.Slerp(reflectedFwd, tip.forward, dirBlend);

            if(isUsing)
            {
                GroundSystem.ApplyBurnt(hit);
            }
        }

        var blend = 1f - Mathf.Pow(1f - tipSmoothing, Time.deltaTime * 60);
        fwd = Vector3.Slerp(fwd, targetFwd, blend);
        particleTr.forward = fwd;

        if (isUsing && !fireSource.isPlaying) fireSource.Play();
        if (!isUsing && fireSource.isPlaying) fireSource.Stop();
    }

    public override void OnEndUse ()
    {
        particles.Stop(true);
    }
}
