using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ElementSlider : Element
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;
    public Slider slider;
    public Transform nodeParent;
    public RectTransform nodeTemplate;

    private float[] nodes;
    private char suffix;
    private int padding = 1;
    public void Build (string fieldName, Type fieldType, string label, float[] nodes, float min, float max, int padding, char suffix)
    {
        this.fieldName = fieldName;
        this.fieldType = fieldType;
        slider.minValue = min;
        slider.maxValue = max;
        this.padding = padding;
        this.suffix = suffix;
        this.nodes = nodes;

        labelText.SetText(label);

        if(nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                RectTransform newNode = Instantiate(nodeTemplate, nodeParent);
                newNode.gameObject.SetActive(true);
                newNode.anchorMin = Vector2.right * Mathf.InverseLerp(min, max, nodes[i]) + Vector2.up * 0.5f;
                newNode.anchorMax = newNode.anchorMin;
                newNode.anchoredPosition = Vector2.zero;
            }
        }

        OnValueChanged(min);
    }

    public override void SetValue (object value)
    {
        slider.value = float.TryParse(value.ToString(), out float result) ? result : 0f;
    }

    public override object GetValue ()
    {
        return slider.value;
    }

    public void OnValueChanged (float value)
    {
        float mul = 1f;
        if (suffix == '%') mul = 100;

        if (nodes != null)
        {
            float smallestDistance = Mathf.Infinity;
            int closestNode = -1;
            for (int i = 0; i < nodes.Length; i++)
            {
                float dist = Mathf.Abs(value - nodes[i]);
                if(dist < smallestDistance)
                {
                    smallestDistance = dist;
                    closestNode = i;
                }
            }
            if(closestNode != - 1 && Mathf.InverseLerp(slider.minValue, slider.maxValue, smallestDistance) < 0.01f)
            {
                value = nodes[closestNode];
            }
        }
        else
        {
            value = Mathf.Round(value * mul * Mathf.Pow(10, padding)) / Mathf.Pow(10, padding) / mul;
        }

        valueText.SetText(((value * mul).ToString("F" + padding.ToString())) + suffix);
        slider.value = value;
    }
}
