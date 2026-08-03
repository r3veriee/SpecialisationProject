using UnityEngine;
using TMPro;
using System.Collections;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;
    public Color runningColor = Color.white;
    public Color finishedColor = Color.green;
    public Color failedColor = Color.red;

    public float currentTime = 0f;
    private bool isRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (timerText != null) timerText.gameObject.SetActive(false);
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
        timerText.gameObject.SetActive(true);
        currentTime = 0f;
        isRunning = true;
        if (timerText != null) timerText.color = runningColor;
    }

    public void StopTimer()
    {
        if (!isRunning) return;
        isRunning = false;
        if (timerText != null) timerText.color = finishedColor;
        StartCoroutine(ShutdownTimerSequence());
    }

    IEnumerator ShutdownTimerSequence()
    {
        yield return new WaitForSeconds(3f);
        if (timerText != null) timerText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void CancelTimer()
    {
        isRunning = false;
        if (timerText != null)
        {
            timerText.color = failedColor;
            timerText.text = "RUN FAILED";
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
}