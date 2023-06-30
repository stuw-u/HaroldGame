using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ElementHeader : Element
{
    public TextMeshProUGUI textUGUI;

    public void Build (string text)
    {
        textUGUI.SetText(text);
    }
}
