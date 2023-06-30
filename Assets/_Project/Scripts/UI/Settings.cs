using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[DefaultExecutionOrder(-1)]
public class Settings : MonoBehaviour
{
    public static event Action<SettingsData> onSettingsUpdate;
    public static SettingsData data { 
        get
        {
            if(inst == null)
            {
                if(!warningShown) Debug.LogWarning("Settings manager doesn't exist yet!");
                warningShown = true;
                return new SettingsData();
            }
            return inst.settings;
        }
    }

    private SettingsData settings;
    private string datapath;
    private static bool warningShown;
    private static Settings inst;
    void Start ()
    {
        if(inst != null && inst.gameObject != gameObject)
        {
            Destroy(inst);
        }
        inst = this;
        DontDestroyOnLoad(inst.gameObject);
        datapath = Application.persistentDataPath + "/settings.json";

        LoadFromFile();
        ApplyMainSettings();

        onSettingsUpdate?.Invoke(settings);
    }

    public static void ApplyDisplaySettings (SettingsData s)
    {
        if (inst == null)
        {
            Debug.LogWarning("Settings manager doesn't exist yet!");
            return;
        }

        inst.StartCoroutine(inst.MoveToDisplay(s.targetScreen, () => {
            bool isDefaultResolution = s.resolution.w == 0 || s.resolution.h == 0;
            FullScreenMode fullScreenMode = s.windowMode switch
            {
                WindowMode.Fullscreen => FullScreenMode.ExclusiveFullScreen,
                WindowMode.FullscreenWindow => FullScreenMode.FullScreenWindow,
                WindowMode.Windowed => FullScreenMode.Windowed,
                _ => FullScreenMode.Windowed
            };

            if (isDefaultResolution)
            {
                List<DisplayInfo> displayInfos = new();
                Screen.GetDisplayLayout(displayInfos);
                int targetScreen = s.targetScreen;
                if (targetScreen >= displayInfos.Count) targetScreen = 0;
                var display = displayInfos[targetScreen];

                if(fullScreenMode == FullScreenMode.Windowed){
                    Screen.SetResolution(display.width / 2, display.height / 2, FullScreenMode.Windowed);
                }
                else
                {
                    Screen.SetResolution(display.width, display.height, fullScreenMode);
                }
            }
            else
            {
                Screen.SetResolution(s.resolution.w, s.resolution.h, fullScreenMode);
            }
            
        }));
    }

    public static void ApplyMainSettings ()
    {
        if (inst == null)
        {
            Debug.LogWarning("Settings manager doesn't exist yet!");
            return;
        }
        Application.targetFrameRate = inst.settings.maxFramerate switch
        {
            MaxFramerate.FPS30 => 30,
            MaxFramerate.FPS45 => 45,
            MaxFramerate.FPS60 => 60,
            MaxFramerate.FPS72 => 72,
            MaxFramerate.FPS120 => 120,
            MaxFramerate.FPS144 => 144,
            MaxFramerate.FPS240 => 240,
            _ => 0,
        };
        QualitySettings.vSyncCount = inst.settings.enableVsync ? 1 : 0;
        onSettingsUpdate?.Invoke(inst.settings);
    }

    private IEnumerator MoveToDisplay (int index, System.Action afterAction)
    {
        try
        {
            List<DisplayInfo> displayInfos = new();
            Screen.GetDisplayLayout(displayInfos);
            if(index >= displayInfos.Count)
            {
                index = 0;
            }
            var display = displayInfos[index];
            if(Screen.mainWindowDisplayInfo.name != display.name)
            {
                Debug.Log($"Moving window to {display.name}");

                Vector2Int targetCoordinates = new (0, 0);
                if (Screen.fullScreenMode != FullScreenMode.Windowed)
                {
                    // Target the center of the display. Doing it this way shows off
                    // that MoveMainWindow snaps the window to the top left corner
                    // of the display when running in fullscreen mode.
                    targetCoordinates.x += display.width / 2;
                    targetCoordinates.y += display.height / 2;
                }

                var moveOperation = Screen.MoveMainWindowTo(display, targetCoordinates);
                yield return moveOperation;
            }
        }
        finally
        {
            // Reset canvas
            
            afterAction();
        }
    }

    public static void Save (SettingsData settings)
    {
        if (inst == null)
        {
            Debug.LogWarning("Settings manager doesn't exist yet!");
            return;
        }

        inst.SaveToFile(settings);
        inst.settings = settings;
    }

    private void LoadFromFile ()
    {
        if(!File.Exists(datapath))
        {
            settings = new SettingsData();
            SaveToFile(settings);
            return;
        }

        try
        {
            settings = JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(datapath));
        }
        catch
        {
            Debug.LogError("Failed to load settings");
            settings = new SettingsData();
        }
    }

    private void SaveToFile (SettingsData newSettings)
    {
        try
        {
            JsonSerializer serializer = new();
            serializer.Formatting = Formatting.Indented;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.MaxDepth = 1;
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            using (StreamWriter sw = new(datapath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, newSettings);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not save settings: {e}");
        }
        finally
        {
            settings = newSettings;
        }
        
    }
}
