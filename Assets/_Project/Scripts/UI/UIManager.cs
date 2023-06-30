using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject introPanel;
    [SerializeField] GameObject gameplayPanel;
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject pausePanel;

    [SerializeField] CanvasGroup warning;
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI timer;
    [SerializeField] TextMeshProUGUI timerOutline;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] float gameOverAnimSpeed = 0.5f;
    [SerializeField] AnimationCurve gameOverCurve;
    [SerializeField] RectTransform gameOverTransform;
    [SerializeField] TextMeshProUGUI scoreDisplay;

    [SerializeField] TextMeshProUGUI superScore1;
    [SerializeField] TextMeshProUGUI superScore2;

    private float superScoreTime;
    private float time;
    private float gameOverAnimValue;
    private bool isDuringGameEnd = false;
    private bool isDuringReset = false;
    static UIManager inst;
    private void Awake ()
    {
        inst = this;

        if(PlayerPrefs.GetInt("HasSeenIntro", -1) == -1)
        {
            introPanel.SetActive(true);
            menuPanel.SetActive(true);
            PlayerPrefs.SetInt("HasSeenIntro", 1);
        }
        else
        {
            menuPanel.SetActive(true);
            introPanel.SetActive(false);
        }
        gameplayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public static void OnGameOver (int score, int highscore)
    {
        bool isHighscore = score > highscore;

        inst.gameOverTransform.localScale = Vector3.zero;
        inst.isDuringGameEnd = true;
        inst.gameOverPanel.SetActive(true);
        if(isHighscore)
        {
            inst.scoreDisplay.SetText($"new high score : {score.ToString("D3")}");
        }
        else
        {
            inst.scoreDisplay.SetText($"score : {score.ToString("D3")}, highscore: {highscore.ToString("D3")}");
        }
    }

    public void Unpause ()
    {
        inst.gameplayPanel.SetActive(true);
        inst.pausePanel.SetActive(false);
        GameManager.Unpause();
    }

    public void ExitToDesktop ()
    {
        Application.Quit();
    }

    public static void OpenPauseMenu ()
    {
        inst.gameplayPanel.SetActive(false);
        inst.pausePanel.SetActive(true);
    }

    private void FixedUpdate ()
    {
        int seconds = Mathf.FloorToInt(time);
        int minutes = seconds / 60;
        seconds -= minutes * 60;

        timer.SetText($"{minutes}:{seconds.ToString("D2")}");
        timerOutline.SetText(timer.text);

        if(GameManager.IsPlayerInControl)
        {
            time += Time.deltaTime;
        }
    }

    private void Update ()
    {
        if(EnemyManager.InDanger())
        {
            warning.alpha = (Mathf.Sin(Time.time * 4f * Mathf.PI) * 0.5f + 0.5f) * 0.4f;
        }
        else
        {
            warning.alpha = 0f;
        }

        if (isDuringGameEnd)
        {
            gameOverAnimValue = Mathf.Clamp01(gameOverAnimValue + Time.deltaTime * gameOverAnimSpeed);
            float value = gameOverCurve.Evaluate(gameOverAnimValue);

            gameOverTransform.localScale = Vector3.one * value;
            gameOverTransform.localRotation = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(-360f, 0, value));

            if(Input.GetKeyDown(KeyCode.Escape) && gameOverAnimValue >= 1)
            {
                OnContinue();
            }
        }
        if (isDuringReset)
        {
            gameOverAnimValue = Mathf.Clamp01(gameOverAnimValue - Time.deltaTime * gameOverAnimSpeed);
            float value = gameOverCurve.Evaluate(gameOverAnimValue);

            gameOverTransform.localScale = Vector3.one * value;
            gameOverTransform.localRotation = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(-360f, 0, value));

            if(gameOverAnimValue <= 0)
            {
                SetScore(0);
                time = 0;
                isDuringReset = false;
                GameManager.ResetGame();
                gameOverPanel.SetActive(false);
            }
        }

        if(superScoreTime > 0f)
        {
            superScoreTime -= Time.deltaTime * 1f;
            superScore1.color = new Color(1, 1, 1, Mathf.InverseLerp(0f, 0.2f, superScoreTime) * Mathf.InverseLerp(1f, 0.8f, superScoreTime));
            superScore2.color = new Color(1, 1, 1, superScoreTime * superScore1.color.a);
            superScore2.transform.localScale = Vector3.one * (1 + (1 - superScoreTime));

            if (superScoreTime <= 0f)
            {
                superScoreTime = 0f;
                superScore1.gameObject.SetActive(false);
                superScore2.gameObject.SetActive(false);
            }
        }
    }

    public void OnContinue ()
    {
        isDuringGameEnd = false;
        isDuringReset = true;
    }

    public static void SetScore (int score)
    {
        if (inst == null) return;

        inst.score.SetText($"CRATES COLLECTED : {score.ToString("D3")}");

        if(score > 0 && score % 5 == 0)
        {
            inst.superScore1.SetText($"{score} x");
            inst.superScore2.SetText($"{score} x");
            inst.superScoreTime = 1f;
            inst.superScoreTime = 1f;
            inst.superScore1.gameObject.SetActive(true);
            inst.superScore2.gameObject.SetActive(true);
            AudioManager.Play(AudioClipName.SuperScore, Vector3.zero, true);
        }
    }

    private void OnDestroy ()
    {
        inst = null;
    }
}
