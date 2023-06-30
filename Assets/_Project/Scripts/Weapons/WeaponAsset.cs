using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New weapon", menuName = "Custom/Weapon")]
public class WeaponAsset : ScriptableObject
{
    public string displayName;
    public Weapon prefab;
    public int scoreToUnlock = 0;
}
