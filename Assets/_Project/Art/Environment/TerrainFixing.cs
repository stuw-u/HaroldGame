using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFixing : MonoBehaviour
{
    [SerializeField] Texture2D fix;
    [SerializeField] float scale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        var terrain = GetComponent<Terrain>();
        var mpb = new MaterialPropertyBlock();
        terrain.GetSplatMaterialPropertyBlock(mpb);
        mpb.SetVector("_Splat0_ST", new Vector4(scale * 500, scale * 500, 0, 0));
        mpb.SetTexture("_Splat0", fix);
        terrain.SetSplatMaterialPropertyBlock(mpb);
    }
}
