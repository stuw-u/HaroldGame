using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

public class ElementToggle : Element
{
    public TextMeshProUGUI labelText;
    public Toggle toggle;

    public void Build (string fieldName, Type fieldType, string label)
    {
        this.fieldName = fieldName;
        this.fieldType = fieldType;

        labelText.SetText(label);
    }

    public override void SetValue (object value)
    {
        toggle.isOn = bool.TryParse(value.ToString(), out bool result) && result;
    }

    public override object GetValue ()
    {
        return toggle.isOn;
    }
}
