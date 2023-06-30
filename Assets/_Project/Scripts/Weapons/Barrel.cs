using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : Weapon
{
    [SerializeField] float shootProjectileSpeed = 5f;
    [SerializeField] float shootingSpeed = 2f;
    [SerializeField] ParticleSystem loadParticle;
    [SerializeField] WasteProjectile projectilePrefab;
    [SerializeField] Transform shootTip;
    [SerializeField] Animation animation;

    private void Awake ()
    {
        animation["Shoot"].speed = shootingSpeed;
    }
    public override void OnBeginUse ()
    {
        animation.Play("Shoot");
        animation.wrapMode = WrapMode.Loop; 
    }

    public override void OnEndUse ()
    {
        animation.CrossFadeQueued("BarrelIdle", 0.3f, QueueMode.PlayNow);
    }

    public void OnShootLoad ()
    {
        loadParticle.Play();
        AudioManager.Play(AudioClipName.WasteBucketSwoosh, transform.position);
        Instantiate(projectilePrefab).Shoot(shootTip.position, shootTip.forward * shootProjectileSpeed);
    }
}
