using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SpirePortal : MonoBehaviour
{
    [Header("Next Level")]
    public string nextSceneName = "Level2";
    public float fadeSpeed = 1.5f;
    public float timeToReadScore = 4f;

    [Header("End Screen UI")]
    public Image blackScreen;
    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI finalTimeText;

    private bool isTransitioning = false;

    void Start()
    {
        // Make sure the end screen is hidden while playing
        if (blackScreen != null) blackScreen.gameObject.SetActive(false);
        if (levelCompleteText != null) levelCompleteText.gameObject.SetActive(false);
        if (finalTimeText != null) finalTimeText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(LevelCompleteSequence(other.gameObject));
        }
    }

    IEnumerator LevelCompleteSequence(GameObject player)
    {
        // Freeze the Player & Stop the Global Timer
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        if (TimeTrialManager.Instance != null) TimeTrialManager.Instance.StopTimer();

        // Fade completely to black
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                blackScreen.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // Turn on the Scoreboard Text
        if (levelCompleteText != null) levelCompleteText.gameObject.SetActive(true);
        if (finalTimeText != null && TimeTrialManager.Instance != null)
        {
            finalTimeText.gameObject.SetActive(true);
            finalTimeText.text = "FINAL TIME:\n" + TimeTrialManager.Instance.timerText.text;
        }

        yield return new WaitForSeconds(timeToReadScore);

        // Load the Spire
        SceneManager.LoadScene(nextSceneName);
    }
}