using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class SettingsData
{
    // General
    public MaxFramerate maxFramerate = MaxFramerate.FPS144;
    public WindowMode windowMode = WindowMode.Windowed;
    public ResolutionPair resolution = new(0, 0);
    public int targetScreen = 0;
    public float uiScaling = 1f;
    public bool enableVsync = true;
    public bool showFPS = false;
    public bool showPing = false;
    public float screenshake = 1f;
    public bool hideCustomContent = false;

    // Peformance
    public float renderScale = 1f;
    public Quality2Steps terrainQuality = Quality2Steps.High;
    public Quality3Steps waterQuality = Quality3Steps.High;
    public Quality2StepsOff shadows = Quality2StepsOff.High;
    public Quality2StepsOff bloom = Quality2StepsOff.High;
    public Quality2StepsOff depthOfField = Quality2StepsOff.High;
    public Quality2StepsOff antialiasing = Quality2StepsOff.High;
    public Quality2Steps details = Quality2Steps.High;

    // Audio
    public float music = 0.5f;
    public float sfx = 0.5f;
    public float playerScreeches = 0.5f;
    public bool muteWhenHidden = false;
    public bool soundOnPlayerJoins = true;
    public bool soundOnGameStarts = true;

    // Controls
    //WIP
    public float sensibility = 1f;
    public bool laptopMode = false;

}

public enum MaxFramerate { FPS30, FPS45, FPS60, FPS72, FPS120, FPS144, FPS240, Unlimited }

public enum WindowMode { Windowed, FullscreenWindow, Fullscreen }

public enum Quality4Steps { High, Medium, Low, None }

public enum Quality3Steps { High, Medium, Low }

public enum Quality2Steps { High, Low }

public enum Quality2StepsOff { High, Low, Off }

public struct ResolutionPair {
    public int w, h; public ResolutionPair (int w, int h) { this.w = w; this.h = h; }
}
