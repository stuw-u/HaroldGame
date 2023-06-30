using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : MonoBehaviour, IHitbox, IFlammable, IDeathListener
{
    [SerializeField] ParticleSystem flameParticles;
    [SerializeField] Enemy enemyHook;
    [SerializeField] MineProjectile projectileHook;

    private bool isLitUp = false;
    public void ApplyFireHit ()
    {
        if(enemyHook) {
            if (enemyHook.IsDead) return;

            enemyHook.ApplyFireHit();
        }
        else if(projectileHook)
        {
            projectileHook.Ignite();
        }

        if (isLitUp) return;

        isLitUp = true;
        if (flameParticles) { flameParticles.Play(); }
    }

    public void Restore ()
    {
        isLitUp = false;
        flameParticles.Stop();
    }

    public void OnDeath ()
    {
        flameParticles.Stop();
    }
}
