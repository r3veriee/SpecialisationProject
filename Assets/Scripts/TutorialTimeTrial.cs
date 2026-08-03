using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialTimeTrial : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public float fakeTimer = 15f;
    public TextMeshProUGUI tutorialTimerText;

    [Header("Failure Settings")]
    public DeathSequenceManager deathManager;
    [Tooltip("Drag the Start Gate and Yellow Pillars here so they turn back on when the player dies!")]
    public GameObject[] objectsToReset;

    private bool timerActive = false;
    private float startingTime;

    void Start()
    {
        startingTime = fakeTimer;
    }

    void Update()
    {
        if (timerActive && fakeTimer > 0)
        {
            fakeTimer -= Time.deltaTime;
            tutorialTimerText.text = "TIME: " + Mathf.Ceil(fakeTimer).ToString();
        }
        else if (timerActive && fakeTimer <= 0)
        {
            // The timer has run out!
            timerActive = false;
            tutorialTimerText.text = "TIME: 0";

            // Kill the Player
            if (deathManager != null)
            {
                deathManager.TriggerDeath();
            }

            // Reset the room so they can try again from the checkpoint
            ResetTutorialRoom();
        }
    }

    public void StartTutorialTimer()
    {
        timerActive = true;
        tutorialTimerText.gameObject.SetActive(true);
    }

    public void AddTime(float amount)
    {
        fakeTimer += amount;
        StartCoroutine(FlashTextGreen());
    }

    public void StopTutorialTimer()
    {
        if (!timerActive) return;

        timerActive = false;
        tutorialTimerText.color = Color.yellow;
        tutorialTimerText.text = "TUTORIAL CLEARED!";

        StartCoroutine(ShutdownTimerSequence());
    }

    IEnumerator ShutdownTimerSequence()
    {
        yield return new WaitForSeconds(2f);
        if (tutorialTimerText != null) tutorialTimerText.gameObject.SetActive(false);
        this.enabled = false;
    }

    IEnumerator FlashTextGreen()
    {
        tutorialTimerText.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        tutorialTimerText.color = Color.white;
    }

    public void ResetTutorialRoom()
    {
        fakeTimer = startingTime;

        if (tutorialTimerText != null)
            tutorialTimerText.gameObject.SetActive(false);

        foreach (GameObject obj in objectsToReset)
        {
            if (obj != null) obj.SetActive(true);
        }
    }
}