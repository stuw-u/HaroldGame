using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class IntroDialogue : MonoBehaviour
{
    [SerializeField] float timeBetweenLetters = 0.05f;
    [SerializeField] float fadeOutSpeed = 1f;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image bg;

    [SerializeField, TextArea] string[] sections;

    StringBuilder sb = new();
    private bool isLoadingText = false;
    int currentSection = 0;
    float fadeOut = 0;
    bool exit;
    private void OnEnable ()
    {
        int i = 0;
        foreach (var section in sections)
        {
            sections[i] = section.Replace("{year}", (System.DateTime.Now.Year + 40).ToString());
            i++;
        }
        StartCoroutine(TextAnimator(0));
    }

    private void Update ()
    {
        if(!isLoadingText && Input.GetMouseButtonDown(0))
        {
            if(currentSection + 1 < sections.Length)
            {
                currentSection++;
                StartCoroutine(TextAnimator(currentSection));
            }
            else
            {
                exit = true;
            }
        }

        if(exit)
        {
            text.color = new Color(1, 1, 1, Mathf.InverseLerp(0.5f, 0f, fadeOut));
            bg.color = new Color(0, 0, 0, Mathf.InverseLerp(1f, 0.5f, fadeOut));

            fadeOut += Time.deltaTime * fadeOutSpeed;
            if(fadeOut > 1f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    const string separator = "<color=#00000000>";
    int tag = 0;
    IEnumerator TextAnimator (int section)
    {
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        isLoadingText = true;
        for (int i = 0; i < sections[section].Length; i++)
        {
            if (sections[section][i] == '\\')
            {
                continue;
            }
            if (sections[section][i] == '<')
            {
                tag++;
            }
            if (sections[section][i] == '>')
            {
                tag--;
            }

            if(tag > 0) { continue; }

            sb.Clear();
            for(int j = 0; j < sections[section].Length; j++)
            {
                sb.Append(sections[section][j]);
                if(j == i)
                {
                    sb.Append(separator);
                }
            }
            text.SetText(sb);
            yield return new WaitForSeconds(Input.GetMouseButton(0) ? 0 : timeBetweenLetters);
        }
        sb.Append("<color=#222F> >>>>>");
        text.SetText(sb);
        isLoadingText = false;
    }
}
