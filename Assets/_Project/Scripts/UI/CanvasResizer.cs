using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasResizer : SettingsUpdater
{
    public CanvasScaler canvasScaler;

    protected override void OnUpdateSettings (SettingsData settings)
    {
            canvasScaler.scaleFactor = settings.uiScaling;
    }
}
