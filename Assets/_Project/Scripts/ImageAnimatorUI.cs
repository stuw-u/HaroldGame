using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimatorUI : MonoBehaviour
{

    [SerializeField] double FPS = 35f;
    [SerializeField] Sprite[] frames;
    [SerializeField] Image outputRenderer;

    void Update ()
    {
        int frame = (int)Math.Floor(Time.unscaledTimeAsDouble * FPS);
        frame = frame % frames.Length;

        outputRenderer.sprite = frames[frame];
    }
}
