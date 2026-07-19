using UnityEngine;
using TMPro;
using System.Collections;

public class GameFinaleManager : MonoBehaviour
{
    [Header("Cinematic Cameras")]
    public Camera mainPlayerCam;
    [Tooltip("A new camera placed high above the tower looking straight down.")]
    public Camera topDownCinematicCam;
    [Tooltip("An empty GameObject in the exact center of the tower. We will spin this!")]
    public Transform cameraPivot;
    public float cameraSpinSpeed = 25f;

    [Header("Visuals & UI")]
    public CanvasGroup blackFadeScreen;
    public TextMeshProUGUI thankYouText;
    public float timeBeforeFadeStarts = 2f;
    public float fadeSpeed = 0.5f;

    private bool finaleTriggered = false;

    void Start()
    {
        if (topDownCinematicCam != null) topDownCinematicCam.enabled = false;

        // Hide the UI at the start
        if (thankYouText != null)
        {
            thankYouText.gameObject.SetActive(false);
            thankYouText.color = new Color(1, 1, 1, 0);
        }
        if (blackFadeScreen != null) blackFadeScreen.alpha = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !finaleTriggered)
        {
            StartCoroutine(FinaleRoutine(other.gameObject));
        }
    }

    IEnumerator FinaleRoutine(GameObject player)
    {
        finaleTriggered = true;

        // STOP THE GLOBAL TIMER
        if (TimeTrialManager.Instance != null)
        {
            TimeTrialManager.Instance.StopTimer();
        }

        // Hide the player completely
        player.SetActive(false);

        // Swap to the spinning camera
        if (mainPlayerCam != null) mainPlayerCam.enabled = false;
        if (topDownCinematicCam != null)
        {
            topDownCinematicCam.enabled = true;
            topDownCinematicCam.transform.SetParent(cameraPivot);
        }

        // Trigger the Chromatic burst
        ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
        if (decayScript != null) decayScript.TriggerColourBurst(5f);

        // Spin while letting them look at the tower
        float timer = 0f;
        while (timer < timeBeforeFadeStarts)
        {
            if (cameraPivot != null) cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Slowly fade to black while still spinning
        float alpha = 0f;
        while (alpha < 1f)
        {
            if (cameraPivot != null) cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
            alpha += Time.deltaTime * fadeSpeed;
            if (blackFadeScreen != null) blackFadeScreen.alpha = alpha;
            yield return null;
        }

        // Fade in the "Thank You" text AND the Final Score
        if (thankYouText != null)
        {
            thankYouText.gameObject.SetActive(true);

            // Append the final time from the UI manager onto the end screen
            if (TimeTrialManager.Instance != null)
            {
                thankYouText.text = "THANK YOU FOR PLAYING\n\nFINAL TIME: " + TimeTrialManager.Instance.timerText.text;
            }

            float textAlpha = 0f;
            while (textAlpha < 1f)
            {
                textAlpha += Time.deltaTime * fadeSpeed;
                thankYouText.color = new Color(1, 1, 1, textAlpha);
                yield return null;
            }
        }
    }
}