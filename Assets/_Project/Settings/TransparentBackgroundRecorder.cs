using UnityEngine;
using System.Collections;
using System.IO;

/*
Usage:
1. Attach this script to your chosen camera's game object.
2. Set that camera's Clear Flags field to Solid Color.
3. Use the inspector to set frameRate and framesToCapture
4. Choose your desired resolution in Unity's Game window (must be less than or equal to your screen resolution)
5. Turn on "Maximise on Play"
6. Play your scene. Screenshots will be saved to YourUnityProject/Screenshots by default.
*/

public class TransparentBackgroundRecorder : MonoBehaviour
{

    #region public fields
    [Tooltip("A folder will be created with this base name in your project root")]
    public string folderBaseName = "Screenshots";
    [Tooltip("How many frames should be captured per second of game time")]
    public int frameRate = 24;
    [Tooltip("How many frames should be captured before quitting")]
    public int framesToCapture = 24;
    public RenderTexture rt;
    #endregion
    #region private fields
    private Camera mainCam;
    private Texture2D target2D;
    private int videoFrame = 0; // how many frames we've rendered
    private float originalTimescaleTime;
    private bool done = false;
    #endregion

    void Awake ()
    {
        mainCam = gameObject.GetComponent<Camera>();
        mainCam.targetTexture = rt;
        originalTimescaleTime = Time.timeScale;
        target2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        Time.captureFramerate = frameRate;
    }

    void LateUpdate ()
    {
        if (!done)
        {
            StartCoroutine(CaptureFrames());
        }
        else
        {
            Debug.Log("Complete! "+videoFrame+" videoframes rendered. File names are 0 indexed)");
            Debug.Break();
        }
    }

    IEnumerator CaptureFrames ()
    {
        yield return new WaitForEndOfFrame();
        if (videoFrame < framesToCapture)
        {
            CaptureFrame();
            Debug.Log("Rendered frame " +videoFrame);
            videoFrame++;
        }
        else
        {
            done=true;
            StopCoroutine("CaptureFrame");
        }
    }

    void CaptureFrame ()
    {
        mainCam.Render();
        SavePng("shot");
    }

    void SavePng (string n = "shot")
    {
        RenderTexture.active = rt;
        target2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        target2D.Apply();
        var pngShot = target2D.EncodeToPNG();
        File.WriteAllBytes($"{folderBaseName}/{videoFrame:D04} {n}.png", pngShot);
    }
}
