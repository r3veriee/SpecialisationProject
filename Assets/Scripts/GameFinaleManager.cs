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
    public CanvasGroup blackFadeScreen; // Your existing fade screen CanvasGroup
    public TextMeshProUGUI thankYouText;
    public float timeBeforeFadeStarts = 2f;
    public float fadeSpeed = 0.5f;

    private bool finaleTriggered = false;

    void Start()
    {
        if (topDownCinematicCam != null) topDownCinematicCam.enabled = false;
        if (thankYouText != null) thankYouText.color = new Color(1, 1, 1, 0); // Hide text
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

        player.SetActive(false);

        mainPlayerCam.enabled = false;
        topDownCinematicCam.enabled = true;

        topDownCinematicCam.transform.SetParent(cameraPivot);

        ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
        if (decayScript != null) decayScript.TriggerColourBurst(5f);

        float timer = 0f;
        while (timer < timeBeforeFadeStarts)
        {
            cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        float alpha = 0f;
        while (alpha < 1f)
        {
            cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
            alpha += Time.deltaTime * fadeSpeed;
            if (blackFadeScreen != null) blackFadeScreen.alpha = alpha;
            yield return null;
        }

        if (thankYouText != null)
        {
            thankYouText.gameObject.SetActive(true);
            float textAlpha = 0f;
            while (textAlpha < 1f)
            {
                textAlpha += Time.deltaTime;
                thankYouText.color = new Color(1, 1, 1, textAlpha);
                yield return null;
            }
        }
    }
}