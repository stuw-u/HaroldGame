using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour, IHitbox
{
    [SerializeField] Enemy enemyHook;

    public void ApplyHit (int damage)
    {
        if (enemyHook)
        {
            enemyHook.ApplyHit(damage);
        }
    }
}
