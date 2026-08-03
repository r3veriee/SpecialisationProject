using UnityEngine;
using TMPro;

public class TutorialTimeTrial : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public float fakeTimer = 15f;
    public TextMeshProUGUI tutorialTimerText;

    private bool timerActive = false;

    void Update()
    {
        if (timerActive && fakeTimer > 0)
        {
            fakeTimer -= Time.deltaTime;
            tutorialTimerText.text = "TIME: " + Mathf.Ceil(fakeTimer).ToString();
        }
        else if (fakeTimer <= 0)
        {
            tutorialTimerText.text = "TIME: 0";
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

    System.Collections.IEnumerator FlashTextGreen()
    {
        tutorialTimerText.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        tutorialTimerText.color = Color.white;
    }
}