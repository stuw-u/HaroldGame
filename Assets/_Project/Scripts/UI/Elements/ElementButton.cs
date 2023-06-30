using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ElementButton : Element
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI buttonLabelText;
    public Button button;

    public void Build (string label, string buttonLabel, Action action)
    {
        labelText.SetText(label);
        buttonLabelText.SetText(buttonLabel);
        button.onClick.AddListener(() => action());
    }
}
