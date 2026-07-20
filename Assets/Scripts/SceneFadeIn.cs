using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag your black Fade Panel's CanvasGroup here")]
    public CanvasGroup fadeScreen;

    [Header("Settings")]
    public float fadeSpeed = 1.5f;

    void Start()
    {
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 1f;
            StartCoroutine(FadeInRoutine());
        }
    }

    IEnumerator FadeInRoutine()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeScreen.alpha = alpha;
            yield return null;
        }
        if (fadeScreen != null) fadeScreen.gameObject.SetActive(false);
    }
}