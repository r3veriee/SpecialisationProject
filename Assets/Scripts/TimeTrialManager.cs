//using UnityEngine;
//using TMPro;

//public class TimeTrialManager : MonoBehaviour
//{
//    public static TimeTrialManager Instance;

//    [Header("UI Reference")]
//    public TextMeshProUGUI timerText; // Drag your UI Text here

//    [Header("Timer State")]
//    public Color runningColor = Color.white;
//    public Color finishedColor = Color.green;

//    private float currentTime = 0f;
//    private bool isRunning = false;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    void Update()
//    {
//        if (isRunning)
//        {
//            currentTime += Time.deltaTime;
//            UpdateTimerDisplay();
//        }
//    }

//    public void StartTimer()
//    {
//        currentTime = 0f;
//        isRunning = true;
//        if (timerText != null) timerText.color = runningColor;
//    }

//    public void StopTimer()
//    {
//        isRunning = false;
//        if (timerText != null) timerText.color = finishedColor;
//    }

//    void UpdateTimerDisplay()
//    {
//        if (timerText == null) return;

//        // Math to format time into MM:SS:MS
//        int minutes = Mathf.FloorToInt(currentTime / 60f);
//        int seconds = Mathf.FloorToInt(currentTime % 60f);
//        float milliseconds = (currentTime * 1000f) % 1000f;

//        // String formatting for that classic speedrun timer look
//        timerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
//    }
//}

using UnityEngine;
using TMPro;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;

    public TextMeshProUGUI timerText;
    public Color runningColor = Color.white;
    public Color finishedColor = Color.green;

    private float currentTime = 0f;
    private bool isRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        isRunning = false;
        if (timerText != null) timerText.color = finishedColor;
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