using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUser : MonoBehaviour
{
    public Transform root;
    public Transform handIKPoint;
    public Transform handRestingPoint;
    public Weapon equippedWeapon;
    private PlayerController pc;
    private bool wasShooting;

    public void Clear ()
    {
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon.gameObject);
        }
    }

    private void Awake ()
    {
        pc = GetComponent<PlayerController>();
        if(equippedWeapon != null)
        {
            equippedWeapon.OnInit();
        }
    }

    void LateUpdate()
    {
        bool shootingInput = Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftShift);
        bool isShooting = shootingInput && GameManager.IsPlayerInControl && equippedWeapon != null;
        if (equippedWeapon != null)
        {
            if (!wasShooting && isShooting)
            {
                equippedWeapon.OnBeginUse();
            }
            if (isShooting)
            {
                equippedWeapon.OnUse();
            }
            else
            {
                equippedWeapon.OnNotUsing();
            }
            equippedWeapon.OnUpdate(isShooting);
            if (wasShooting && !isShooting)
            {
                equippedWeapon.OnEndUse();
            }
            pc.SetShootingState(isShooting);
        }

        handIKPoint.transform.position = (equippedWeapon == null) ? 
            handRestingPoint.transform.position : equippedWeapon.ikPoint.transform.position;
        wasShooting = isShooting;
    }

    public void GiveNewWeapon (WeaponAsset asset)
    {
        if(equippedWeapon != null) Destroy(equippedWeapon.gameObject);
        equippedWeapon = Instantiate(asset.prefab, root);
    }
}
