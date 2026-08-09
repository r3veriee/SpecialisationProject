using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameFinaleManager : MonoBehaviour
{
    [Header("Cameras & FX")]
    public Camera mainPlayerCam;
    public Camera orbitCinematicCam;
    public Transform cameraPivot;
    public float cameraSpinSpeed = 25f;

    public GlitchEffect glitchScript;
    public ChromaticDecay chromaticScript;
    public Material greenFinaleSkybox;

    public CanvasGroup blackFadeScreen;

    [Header("Finale Text Elements")]
    public TextMeshProUGUI thankYouText;

    [Header("Level 1 Stats")]
    public TextMeshProUGUI level1TimeText;
    public TextMeshProUGUI level1DeathsText;

    [Header("Level 2 Stats")]
    public TextMeshProUGUI level2TimeText;
    public TextMeshProUGUI level2DeathsText;

    [Header("Total Stats")]
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI totalDeathsText;

    [Header("UI Cleanup")]
    public GameObject[] uiElementsToHide;

    [Header("Finale Material Swap")]
    public Transform materialSwapRoot;
    public Material finaleObjectMaterial;
    public Renderer[] renderersToExcludeFromSwap;

    public float timeBeforeFadeStarts = 2f;
    public float fadeSpeed = 0.5f;

    private bool finaleTriggered = false;

    void Start()
    {
        if (orbitCinematicCam != null) orbitCinematicCam.enabled = false;

        // Ensure all texts start invisible
        HideText(thankYouText);
        HideText(level1TimeText);
        HideText(level1DeathsText);
        HideText(level2TimeText);
        HideText(level2DeathsText);
        HideText(totalTimeText);
        HideText(totalDeathsText);

        if (blackFadeScreen != null) blackFadeScreen.alpha = 0f;
        if (!PlayerPrefs.HasKey("SessionDeaths")) PlayerPrefs.SetInt("SessionDeaths", 0);
    }

    private void HideText(TextMeshProUGUI tmp)
    {
        if (tmp != null)
        {
            tmp.gameObject.SetActive(false);
            tmp.color = new Color(1, 1, 1, 0);
        }
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
        AudioManager.Instance.PlaySFX(SFXType.FinaleEnd);

        if (TimeTrialManager.Instance != null) TimeTrialManager.Instance.StopTimer();

        player.SetActive(false);

        foreach (GameObject ui in uiElementsToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

        if (glitchScript != null) glitchScript.DisableEffect();
        if (chromaticScript != null) chromaticScript.DisableEffect();

        RenderSettings.fog = false;
        RenderSettings.skybox = greenFinaleSkybox;
        DynamicGI.UpdateEnvironment();
        ApplyFinaleMaterials();

        if (orbitCinematicCam != null) orbitCinematicCam.clearFlags = CameraClearFlags.Skybox;

        if (mainPlayerCam != null) mainPlayerCam.enabled = false;
        if (orbitCinematicCam != null)
        {
            orbitCinematicCam.enabled = true;
            orbitCinematicCam.transform.SetParent(cameraPivot);
        }

        float timer = 0f;
        while (timer < timeBeforeFadeStarts)
        {
            SpinCamera();
            timer += Time.deltaTime;
            yield return null;
        }

        float alpha = 0f;
        while (alpha < 1f)
        {
            SpinCamera();
            alpha += Time.deltaTime * fadeSpeed;
            if (blackFadeScreen != null) blackFadeScreen.alpha = alpha;
            yield return null;
        }

        float l2Time = TimeTrialManager.Instance != null ? TimeTrialManager.Instance.currentTime : 0f;
        float l1Time = PlayerPrefs.GetFloat("Level1Time", 0f);

        int l2Deaths = PlayerPrefs.GetInt("SessionDeaths", 0);
        int l1Deaths = PlayerPrefs.GetInt("Level1Deaths", 0);
        int totalDeaths = l1Deaths + l2Deaths;

        // Apply Text Strings
        if (thankYouText != null) { thankYouText.gameObject.SetActive(true); thankYouText.text = "THANK YOU FOR PLAYING"; }

        if (level1TimeText != null) { level1TimeText.gameObject.SetActive(true); level1TimeText.text = "L1 TIME: " + FormatTime(l1Time); }
        if (level1DeathsText != null) { level1DeathsText.gameObject.SetActive(true); level1DeathsText.text = "L1 DEATHS: " + l1Deaths; }

        if (level2TimeText != null && TimeTrialManager.Instance != null) { level2TimeText.gameObject.SetActive(true); level2TimeText.text = "L2 TIME: " + TimeTrialManager.Instance.timerText.text; }
        if (level2DeathsText != null) { level2DeathsText.gameObject.SetActive(true); level2DeathsText.text = "L2 DEATHS: " + l2Deaths; }

        if (totalTimeText != null) { totalTimeText.gameObject.SetActive(true); totalTimeText.text = "TOTAL TIME: " + FormatTime(l1Time + l2Time); }
        if (totalDeathsText != null) { totalDeathsText.gameObject.SetActive(true); totalDeathsText.text = "TOTAL DEATHS: " + totalDeaths; }

        float textAlpha = 0f;
        while (textAlpha < 1f)
        {
            textAlpha += Time.deltaTime * fadeSpeed;

            if (thankYouText != null) thankYouText.color = new Color(1, 1, 1, textAlpha);

            if (level1TimeText != null) level1TimeText.color = new Color(1, 1, 1, textAlpha);
            if (level1DeathsText != null) level1DeathsText.color = new Color(1, 1, 1, textAlpha);

            if (level2TimeText != null) level2TimeText.color = new Color(1, 1, 1, textAlpha);
            if (level2DeathsText != null) level2DeathsText.color = new Color(1, 1, 1, textAlpha);

            if (totalTimeText != null) totalTimeText.color = new Color(1, 1, 1, textAlpha);
            if (totalDeathsText != null) totalDeathsText.color = new Color(1, 1, 1, textAlpha);

            yield return null;
        }

        // Reset memory for the next playthrough
        PlayerPrefs.SetInt("SessionDeaths", 0);
        PlayerPrefs.SetInt("Level1Deaths", 0);
        PlayerPrefs.SetFloat("Level1Time", 0f);

        yield return new WaitForSeconds(7f);
        SceneManager.LoadScene("MainMenu");
    }

    private void SpinCamera()
    {
        if (cameraPivot != null)
        {
            cameraPivot.Rotate(Vector3.up, cameraSpinSpeed * Time.deltaTime);
            if (orbitCinematicCam != null) orbitCinematicCam.transform.LookAt(cameraPivot.position);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
        float fraction = (timeInSeconds * 1000) % 1000;
        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction);
    }

    private void ApplyFinaleMaterials()
    {
        if (materialSwapRoot == null || finaleObjectMaterial == null) return;

        HashSet<Renderer> excluded = new HashSet<Renderer>(renderersToExcludeFromSwap);
        Renderer[] allRenderers = materialSwapRoot.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in allRenderers)
        {
            if (excluded.Contains(r)) continue;

            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = finaleObjectMaterial;
            r.sharedMaterials = mats;
        }
    }
}