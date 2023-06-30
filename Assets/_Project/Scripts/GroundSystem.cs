using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSystem : MonoBehaviour
{
    public Transform player;
    public float rate = 1f;
    public float radius = 16;
    public float fade = 1f;
    public ComputeShader cs;
    public Material mat;
    private RenderTexture rt;
    private int kernel;
    private int kernelC;

    private bool doApplyEffect = false;
    private bool doClear = false;
    private RaycastHit hit;

    private static GroundSystem inst;
    private void Awake () => inst = this;
    private void OnDestroy () => inst = null;

    void Start ()
    {
        rt = new RenderTexture(256, 256, 0);
        rt.enableRandomWrite = true;
        rt.format = RenderTextureFormat.R8;
        kernel = cs.FindKernel("Update");
        kernelC = cs.FindKernel("Clear");
        cs.SetTexture(kernel, "Result", rt);
        cs.SetTexture(kernelC, "Result", rt);
        mat.SetTexture("_RenderTexture", rt);
    }

    public static void ApplyBurnt (RaycastHit hit)
    {
        if (inst == null) return;

        inst.doApplyEffect = true;
        inst.hit = hit;
    }

    public static void Clear ()
    {
        if (inst == null) return;

        inst.doClear = true;
    }

    void FixedUpdate()
    {
        Vector2 pos = -Vector2.one * rt.width;
        if(doApplyEffect)
        {
            pos = hit.lightmapCoord * new Vector2(rt.width, rt.height);
            doApplyEffect = false;

            cs.SetVector("emitter", pos);
            cs.SetFloat("emitterRadius", radius);
            cs.SetFloat("emitterFade", fade);
            cs.SetFloat("rate", Time.deltaTime * rate);
            cs.Dispatch(kernel, rt.width / 8, rt.height / 8, 1);
        }
        if(doClear){
            cs.Dispatch(kernelC, rt.width / 8, rt.height / 8, 1);
            doClear = false;
        }
    }
}
