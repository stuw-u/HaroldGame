using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Weapon
{
    [SerializeField] Transform mineSpawnPoint;
    [SerializeField] MeshRenderer render;
    [SerializeField] GameObject minePrefab;
    [SerializeField] float cooldownSpeed;

    float cooldown;

    public override void OnBeginUse ()
    {
        if(cooldown == 0f)
        {
            AudioManager.Play(AudioClipName.MineDrop, transform.position);
            Instantiate(minePrefab, mineSpawnPoint.position, mineSpawnPoint.rotation);
            cooldown = 1f;
        }
    }

    public override void OnUpdate (bool isUsed)
    {
        if(cooldown > 0f)
        {
            cooldown = Mathf.Clamp01(cooldown - cooldownSpeed * Time.deltaTime);
        }
        render.enabled = cooldown == 0f;
    }

    public override void OnEndUse ()
    {
    }
}
