using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingsUpdater : MonoBehaviour
{
    private void OnEnable ()
    {
        Settings.onSettingsUpdate += OnUpdateSettings;
    }

    private void Start ()
    {
        OnUpdateSettings(Settings.data);
    }

    protected abstract void OnUpdateSettings (SettingsData settings);

    private void OnDisable ()
    {
        Settings.onSettingsUpdate -= OnUpdateSettings;
    }
}
