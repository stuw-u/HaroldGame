using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

[Serializable]
public class LanguagueEntry
{
    public string langWebCode;
    public string languageName;
    public TextAsset textAsset;
}

public class Language : MonoBehaviour
{
    public LanguagueEntry[] languages;

    public static string GetString (string code)
    {
        if (inst == null)
        {
            Debug.LogWarning("Language manager doesn't exist yet!");
            return code;
        }

        return inst.text[code];
    }

    private Dictionary<string, string> text;
    private static Language inst;
    void Awake ()
    {
        if (inst != null)
        {
            Destroy(inst.gameObject);
        }
        inst = this;
        DontDestroyOnLoad(inst.gameObject);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        int langIndex = Array.FindIndex(languages, lang => lang.langWebCode == PlayerPrefs.GetString("lang", "en"));
        TextAsset textAsset = languages[langIndex].textAsset;

        string[] entries = textAsset.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        text = new Dictionary<string, string>();
        foreach(string entry in entries)
        {
            string[] parts = entry.Split("::");
            if(parts.Length < 2)
            {
                Debug.Log($"Failed to load: {entry}");
                continue;
            }
            text.Add(parts[0].TrimStart().TrimEnd(), parts[1].TrimStart().TrimEnd());
        }

        sw.Stop();
        Debug.Log($"Loaded language in {sw.ElapsedMilliseconds} ms");
    }
}
