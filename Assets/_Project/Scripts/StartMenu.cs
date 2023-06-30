using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] float fadeOutSpeed = 1f;
    [SerializeField] Image bg;
    [SerializeField] CanvasGroup canvasGroup;

    float fadeOut = 0;
    bool exit;

    public void Exit()
    {
        exit = true;
        canvasGroup.interactable = false;
        AudioManager.Play(AudioClipName.MenuStart, Vector3.zero, true);
    }

    public void OpenGeorgeLol ()
    {
        Application.OpenURL("https://twitter.com/dogsmagic_");
    }

    void Update()
    {
        if (exit)
        {
            canvasGroup.alpha = Mathf.InverseLerp(0.5f, 0f, fadeOut);
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, Mathf.InverseLerp(1f, 0.5f, fadeOut));

            fadeOut += Time.deltaTime * fadeOutSpeed;
            if (fadeOut > 1f)
            {
                gameObject.SetActive(false);
                GameManager.StartGame();
            }
        }
    }
}
