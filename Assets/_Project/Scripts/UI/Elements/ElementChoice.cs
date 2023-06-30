using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ElementChoice : Element
{
    public bool isDouble;
    public TextMeshProUGUI labelText;
    public Transform parent1;
    public Transform parent2;
    public Button template;
    private Button[] buttons;
    private int selected = 0;

    public void Build (string fieldName, Type fieldType, string label, string[] options, int topRowCount = 0)
    {
        this.fieldName = fieldName;
        this.fieldType = fieldType;

        labelText.SetText(label);

        buttons = new Button[options.Length];

        for(int i = 0; i < options.Length; i++)
        {
            Button newButton;
            if (isDouble && i >= topRowCount)
            {
                newButton = Instantiate(template, parent2);
            }
            else
            {
                newButton = Instantiate(template, parent1);
            }
            newButton.gameObject.SetActive(true);
            newButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(options[i]);
            newButton.GetComponent<ContentSizeFitter>()?.SetLayoutHorizontal();
            int id = i;
            newButton.onClick.AddListener(() => Select(id));
            buttons[i] = newButton;
        }

        Select(0);
    }

    public void Select (int i)
    {
        buttons[selected].interactable = true;
        selected = i;
        buttons[selected].interactable = false;
    }

    public override void SetValue (object value)
    {
        Select((int)Enum.Parse(fieldType, value.ToString()));
    }

    public override object GetValue ()
    {
        return selected;
    }
}
