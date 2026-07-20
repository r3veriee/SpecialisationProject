using UnityEngine;
using TMPro;
using System.Collections;

public class GameFinaleManager : MonoBehaviour
{
    public Camera mainPlayerCam;
    public Camera orbitCinematicCam;
    public Transform cameraPivot;
    public float cameraSpinSpeed = 25f;

    public CanvasGroup blackFadeScreen;
    public TextMeshProUGUI thankYouText;
    public float timeBeforeFadeStarts = 2f;
    public float fadeSpeed = 0.5f;

    private bool finaleTriggered = false;

    void Start()
    {
        if (orbitCinematicCam != null) orbitCinematicCam.enabled = false;

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

        if (TimeTrialManager.Instance != null) TimeTrialManager.Instance.StopTimer();

        player.SetActive(false);

        if (mainPlayerCam != null) mainPlayerCam.enabled = false;
        if (orbitCinematicCam != null)
        {
            orbitCinematicCam.enabled = true;
            orbitCinematicCam.transform.SetParent(cameraPivot);
        }

        ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
        if (decayScript != null) decayScript.TriggerColourBurst(5f);

        float timer = 0f;
        while (timer < timeBeforeFadeStarts)
        {
            if (cameraPivot != null)
            {
                cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
                if (orbitCinematicCam != null) orbitCinematicCam.transform.LookAt(cameraPivot.position);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        float alpha = 0f;
        while (alpha < 1f)
        {
            if (cameraPivot != null)
            {
                cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
                if (orbitCinematicCam != null) orbitCinematicCam.transform.LookAt(cameraPivot.position);
            }
            alpha += Time.deltaTime * fadeSpeed;
            if (blackFadeScreen != null) blackFadeScreen.alpha = alpha;
            yield return null;
        }

        if (thankYouText != null)
        {
            thankYouText.gameObject.SetActive(true);

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