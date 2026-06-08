using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;
    public Color runningColor = Color.white;
    public Color finishedColor = Color.green;

    [Header("Alpha End Sequence")]
    public Image whiteFadeScreen;       
    public TextMeshProUGUI thankYouText;
    public float fadeSpeed = 1f;        

    private float currentTime = 0f;
    private bool isRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure the fade screen is invisible when the game starts
        if (whiteFadeScreen != null) whiteFadeScreen.color = new Color(1, 1, 1, 0);
        if (thankYouText != null) thankYouText.color = new Color(0, 1, 1, 0);
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        currentTime = 0f;
        isRunning = true;
        if (timerText != null) timerText.color = runningColor;
    }

    public void StopTimer()
    {
        if (!isRunning) return;

        isRunning = false;
        if (timerText != null) timerText.color = finishedColor;

        StartCoroutine(AlphaEndRoutine());
    }

    public void CancelTimer()
    {
        if (isRunning)
        {
            isRunning = false;
            if (timerText != null)
            {
                timerText.color = Color.red;
                timerText.text = "RUN FAILED";
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        float milliseconds = (currentTime * 1000f) % 1000f;
        timerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }

    IEnumerator AlphaEndRoutine()
    {
        // Turn on the objects just in case they were disabled
        if (whiteFadeScreen != null) whiteFadeScreen.gameObject.SetActive(true);
        if (thankYouText != null) thankYouText.gameObject.SetActive(true);

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;

            // Fade the background to pure white
            if (whiteFadeScreen != null) whiteFadeScreen.color = new Color(1, 1, 1, alpha);

            // Fade the text
            if (thankYouText != null) thankYouText.color = new Color(0, 1, 1, alpha);

            yield return null;
        }

        // Freeze the game physics entirely so the player stops falling or sliding
        Time.timeScale = 0f;
    }
}