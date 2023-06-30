using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCopyEffect : MonoBehaviour
{
    [SerializeField] Camera cam;

    private void Awake ()
    {
        Copy();

    }

    public void Copy()
    {
        cam.targetTexture?.Release();
        cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cam.Render();
    }
}
