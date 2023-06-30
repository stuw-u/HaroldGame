using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelAnimationCapture : MonoBehaviour
{
    [SerializeField] Barrel barrel;

    public void OnEventFire ()
    {
        barrel.OnShootLoad();
    }
}
