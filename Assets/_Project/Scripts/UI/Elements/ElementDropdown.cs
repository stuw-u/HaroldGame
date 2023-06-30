using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class ElementDropdown : Element
{
    public TextMeshProUGUI labelText;
    public TMP_Dropdown dropdown;

    private List<string> options;
    private List<object> values;
    public void Build (string fieldName, Type fieldType, List<string> options, List<object> values, string label)
    {
        this.fieldName = fieldName;
        this.fieldType = fieldType;
        this.options = options;
        this.values = values;

        labelText.SetText(label);

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    public override void SetValue (object obj)
    {
        int targetValue = -1;
        for (int i = 0; i < values.Count; i++)
        {
            if (obj.Equals(values[i]))
            {
                targetValue = i;
                continue;
            }
        }
        if(targetValue == -1)
        {
            targetValue = values.Count;
            values.Add(obj);
            options.Add(Language.GetString("option.missing"));
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }
        dropdown.value = targetValue;
    }

    public override object GetValue ()
    {
        return values[dropdown.value];
    }
}
