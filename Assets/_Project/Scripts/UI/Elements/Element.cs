using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Element : MonoBehaviour
{
    protected string fieldName;
    protected System.Type fieldType;

    private string description;
    private SettingsMenu menu;

    public void SetDescription (SettingsMenu menu, string description)
    {
        this.menu = menu;
        this.description = description;
    }

    public virtual object GetValue ()
    {
        return null;
    }

    public virtual void SetValue (object value)
    {
    }

    public void Load (JObject settings)
    {
        SetValue(settings[fieldName].ToObject(fieldType));
    }

    public void Save (JObject settings)
    {
        settings[fieldName] = JToken.FromObject(GetValue());
    }

    public void OnHover ()
    {
        if(menu != null && description != null)
        {
            menu.DisplayDescription(description);
        }
    }
}
