using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

using TMPro;
using Object = UnityEngine.Object;

public class SettingsMenu : MonoBehaviour
{
    [Header("Prefabs")]
    public ElementHeader elementHeaderPrefab;
    public GameObject elementDividerPrefab;
    public ElementToggle elementTogglePrefab;
    public ElementChoice elementChoice1Prefab;
    public ElementChoice elementChoice2Prefab;
    public ElementButton elementButtonPrefab;
    public ElementSlider elementSliderPrefab;
    public ElementDropdown elementDropdownPrefab;

    [Header("Tabs")]
    public Sprite tabSelected;
    public Sprite tabUnselected;
    public Button tabsTemplate;
    public Transform tabsParent;
    public GameObject contentTemplate;
    public Transform contentParent;

    [Header("Other")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI applyAndSaveText;
    public TextMeshProUGUI description;

    private SettingsData s;
    private JObject serializedSettings;

    private List<Element> elements;
    private List<Element> elementsScreen;
    private List<Image> tabsImage;
    private List<Button> tabsButton;
    private List<GameObject> contents;
    private int selectedTab = 0;
    int tabCount = 0;
    private Transform currentBuildContent;

    private void Start ()
    {
        tabsImage = new List<Image>();
        tabsButton = new List<Button>();
        contents = new List<GameObject>();
        elements = new List<Element>();
        elementsScreen = new List<Element>();

        s = Settings.data;
        BuildMenu();
        Load();
    }

    private void BuildMenu ()
    {
        title.SetText("Settings");
        applyAndSaveText.SetText("Apply and Save");

        BuildNewCategory("settings.category.general");
        BuildHeader("settings.label.window_options");
        BuildEnum2(
            label: "settings.max_framerate",
            description: "desc.max_framerate",
            labels: new string[] { "fps.30", "fps.45", "fps.60", "fps.72", "fps.120", "fps.144", "fps.240", "fps.unlimited" },
            firstLayer: 4,
            field: nameof(s.maxFramerate), type: typeof(MaxFramerate));
        BuildEnum(
            label: "settings.window_mode",
            description: "desc.window_mode",
            labels: new string[] { "window_mode.windowed", "window_mode.fullscreenWindow", "window_mode.fullscreen" },
            isScreenElement: true,
            field: nameof(s.windowMode), type: typeof(WindowMode));
        BuildResolutionEnum(
            label: "settings.resolution",
            description: "desc.resolution",
            field: nameof(s.resolution), type: typeof(ResolutionPair));
        BuildScreenEnum(
            label: "settings.screen_switch",
            description: "desc.screen_swtich",
            field: nameof(s.targetScreen), type: typeof(int));
        BuildButton(
            label: "settings.apply_window",
            description: "desc.apply_window",
            buttonLabel: "settings.apply",
            () => ApplyWindowSettings());

        BuildDivision();
        BuildHeader("settings.label.general_options");
        BuildSlider(
            label: "settings.ui_scale",
            description: "desc.ui_scale",
            min: 0.5f, max:2f, padding: 2, suffix: 'x',
            points: new float[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f},
            field: nameof(s.uiScaling), type: typeof(float));
        BuildScreenToggle(
            label: "settings.enable_v_sync",
            description: "desc.enable_v_sync",
            field: nameof(s.enableVsync), type: typeof(bool));

        BuildNewCategory("settings.category.performance");
        /*BuildEnum(
            label: "settings.terrain_quality",
            description: "desc.terrain_quality",
            labels: new string[] { "option.high", "option.low" },
            isScreenElement: false,
            field: sType.GetField(nameof(s.terrainQuality)));
        BuildEnum(
            label: "settings.water_quality",
            description: "desc.water_quality",
            labels: new string[] { "option.high", "option.medium", "option.low" },
            isScreenElement: false,
            field: sType.GetField(nameof(s.waterQuality)));*/
        BuildEnum(
            label: "settings.shadows_quality",
            description: "desc.shadows_quality",
            labels: new string[] { "option.high", "option.low", "option.off" },
            isScreenElement: false,
            field: nameof(s.shadows), type: typeof(Quality2StepsOff));
        BuildEnum(
            label: "settings.bloom_quality",
            description: "desc.bloom_quality",
            labels: new string[] { "option.high", "option.low", "option.off" },
            isScreenElement: false,
            field: nameof(s.bloom), type: typeof(Quality2StepsOff));
        BuildEnum(
            label: "settings.details",
            description: "desc.bloom_quality",
            labels: new string[] { "option.high", "option.low" },
            isScreenElement: false,
            field: nameof(s.details), type: typeof(Quality2Steps));
        /*BuildEnum(
            label: "settings.depth_of_field_quality",
            description: "desc.depth_of_field_quality",
            labels: new string[] { "option.high", "option.low", "option.off" },
            isScreenElement: false,
            field: sType.GetField(nameof(s.depthOfField)));
        BuildEnum(
            label: "settings.aa_quality",
            description: "desc.aa_quality",
            labels: new string[] { "option.high", "option.low", "option.off" },
            isScreenElement: false,
            field: sType.GetField(nameof(s.antialiasing)));*/
        BuildSlider(
            label: "settings.render_scale",
            description: "desc.render_scale",
            min: 0.5f, max: 1f, padding: 1, suffix: '%',
            points: new float[] { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f},
            field: nameof(s.renderScale), type: typeof(float));

        BuildNewCategory("settings.category.controls");
        BuildSlider(
            label: "settings.mouse_sensibility",
            description: "desc.music_volume",
            min: 0.2f, max: 2f, padding: 1, suffix: 'x',
            points: new float[] { 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f},
            field: nameof(s.sensibility), type: typeof(float));
        BuildScreenToggle(
            label: "settings.laptop_mode",
            description: "desc.mute_sound_minimized",
            field: nameof(s.laptopMode), type: typeof(bool));
        //BuildHeader("message.come_back");

        BuildNewCategory("settings.category.audio");
        BuildSlider(
            label: "settings.music_volume",
            description: "desc.music_volume",
            min: 0f, max: 1f, padding: 0, suffix: '%',
            points: null,
            field: nameof(s.music), type: typeof(float));
        BuildSlider(
            label: "settings.sfx_volume",
            description: "desc.sfx_volume",
            min: 0f, max: 1f, padding: 0, suffix: '%',
            field: nameof(s.sfx), type: typeof(float));
        /*BuildSlider(
            label: "settings.player_screech_volume",
            description: "desc.player_screech_volume",
            min: 0f, max: 1f, padding: 0, suffix: '%',
            field: sType.GetField(nameof(s.playerScreeches)));*/
        BuildScreenToggle(
            label: "settings.mute_sound_minimized",
            description: "desc.mute_sound_minimized",
            field: nameof(s.muteWhenHidden), type: typeof(bool));
        /*BuildScreenToggle(
            label: "settings.sound_on_join",
            description: "desc.sound_on_join",
            field: sType.GetField(nameof(s.soundOnPlayerJoins)));*/

        SelectTab(0);
    }

    #region Load/Save
    private void Load ()
    {
        serializedSettings = JObject.Parse(JsonConvert.SerializeObject(Settings.data));
        foreach (Element elem in elements)
        {
            elem.Load(serializedSettings);
        }
    }
    #endregion

    #region Interaction
    public void ApplyWindowSettings ()
    {
        SettingsData copyS = new SettingsData();
        JObject serializeCopyS = JObject.Parse(JsonConvert.SerializeObject(copyS));
        foreach (Element elem in elementsScreen)
        {
            elem.Save(serializeCopyS);
        }
        copyS = JsonConvert.DeserializeObject<SettingsData>(serializeCopyS.ToString());
        Settings.ApplyDisplaySettings(copyS);
    }

    public void SaveAndApply ()
    {
        foreach (Element elem in elements)
        {
            elem.Save(serializedSettings);
        }
        s = JsonConvert.DeserializeObject<SettingsData>(serializedSettings.ToString());
        Settings.Save(s);
        Settings.ApplyMainSettings();
    }

    private void SelectTab (int i)
    {
        tabsImage[selectedTab].sprite = tabUnselected;
        tabsButton[selectedTab].interactable = true;
        contents[selectedTab].SetActive(false);
        selectedTab = i;
        tabsImage[selectedTab].sprite = tabSelected;
        tabsButton[selectedTab].interactable = false;
        contents[selectedTab].SetActive(true);
    }

    public void DisplayDescription (string description)
    {
        this.description?.SetText(Language.GetString(description));
        this.description?.GetComponent<ContentSizeFitter>().SetLayoutVertical();
    }
    #endregion

    #region Build
    private void BuildNewCategory (string label)
    {
        Button newTab = Instantiate(tabsTemplate, tabsParent);
        newTab.gameObject.SetActive(true);
        int i = tabCount;
        newTab.onClick.AddListener(() => SelectTab(i));
        newTab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(Language.GetString(label));
        newTab.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
        Image newTabImage = newTab.GetComponent<Image>();
        newTabImage.sprite = tabUnselected;
        tabsImage.Add(newTabImage);
        tabsButton.Add(newTab);

        GameObject content = Instantiate(contentTemplate, contentParent);
        contents.Add(content);
        currentBuildContent = content.transform;

        tabCount++;
    }

    private void BuildHeader (string label)
    {
        ElementHeader newHeader = Instantiate(elementHeaderPrefab, currentBuildContent);
        newHeader.Build(Language.GetString(label));
    }

    private void BuildDivision ()
    {
        GameObject newHeader = Instantiate(elementDividerPrefab, currentBuildContent);
    }

    private void BuildEnum (string label, string description, string[] labels, bool isScreenElement, string field, Type type)
    {
        ElementChoice newElem = Instantiate(elementChoice1Prefab, currentBuildContent);
        newElem.SetDescription(this, description);
        string[] options = new string[labels.Length];
        for(int i = 0; i < options.Length; i++)
        {
           options[i] = Language.GetString(labels[i]);
        }
        newElem.Build(field, type, Language.GetString(label), options, 0);
        elements.Add(newElem);
        if(isScreenElement)
        {
            elementsScreen.Add(newElem);
        }
    }
    private void BuildEnum2 (string label, string description, string[] labels, int firstLayer, string field, Type type)
    {
        ElementChoice newElem = Instantiate(elementChoice2Prefab, currentBuildContent);
        newElem.SetDescription(this, description);
        string[] options = new string[labels.Length];
        for (int i = 0; i < options.Length; i++)
        {
            options[i] = Language.GetString(labels[i]);
        }
        newElem.Build(field, type, Language.GetString(label), options, firstLayer);
        elements.Add(newElem);
    }

    public void BuildResolutionEnum (string label, string description, string field, Type type)
    {
        ElementDropdown newElem = Instantiate(elementDropdownPrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        List<string> options = new();
        List<object> values = new();
        Resolution[] resolutions = Screen.resolutions;
        HashSet<int2> resHashset = new();
        foreach(var res in resolutions)
        {
            int2 resolution = new int2(res.width, res.height);
            if (resHashset.Contains(resolution)) continue;
            resHashset.Add(resolution);

            options.Add($"{res.width} x {res.height}");
            values.Add(new ResolutionPair(res.width, res.height));
        }
        options.Add(Language.GetString("option.default"));
        values.Add(new ResolutionPair(0, 0));
        options.Reverse();
        values.Reverse();

        newElem.Build(field, type, options, values, Language.GetString(label));
        elements.Add(newElem);
        elementsScreen.Add(newElem);
    }

    public void BuildScreenEnum (string label, string description, string field, Type type)
    {
        ElementDropdown newElem = Instantiate(elementDropdownPrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        List<string> options = new();
        List<object> values = new();
        List<DisplayInfo> displays = new();
        Screen.GetDisplayLayout(displays);
        displays.Reverse();
        int i = 0;
        int invI = displays.Count - 1;
        foreach (var display in displays)
        {
            options.Add($"{i}: {display.name}");
            values.Add(invI);
            i++;
            invI--;
        }

        newElem.Build(field, type, options, values, Language.GetString(label));
        elements.Add(newElem);
        elementsScreen.Add(newElem);
    }

    public void BuildScreenToggle (string label, string description, string field, Type type)
    {
        ElementToggle newElem = Instantiate(elementTogglePrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        newElem.Build(field, type, Language.GetString(label));
        elements.Add(newElem);
    }

    private void BuildButton (string label, string description, string buttonLabel, Action action)
    {
        ElementButton newElem = Instantiate(elementButtonPrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        newElem.Build(Language.GetString(label), Language.GetString(buttonLabel), action);
    }

    private void BuildSlider (string label, string description, float min, float max, int padding, char suffix, float[] points, string field, Type type)
    {
        ElementSlider newElem = Instantiate(elementSliderPrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        newElem.Build(field, type, Language.GetString(label), points, min, max, padding, suffix);
        elements.Add(newElem);
    }

    private void BuildSlider (string label, string description, float min, float max, int padding, char suffix, string field, Type type)
    {
        ElementSlider newElem = Instantiate(elementSliderPrefab, currentBuildContent);
        newElem.SetDescription(this, description);
        newElem.Build(field, type, Language.GetString(label), null, min, max, padding, suffix);
        elements.Add(newElem);
    }
    #endregion
}
