using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class GlitchEffect : MonoBehaviour
{
    public Volume postProcessVolume;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        postProcessVolume.profile.TryGet(out lensDistortion);
        postProcessVolume.profile.TryGet(out chromaticAberration);
    }

    public void TriggerGlitch(float duration)
    {
        StartCoroutine(GlitchRoutine(duration));
    }

    IEnumerator GlitchRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Random.Range(-0.5f, 0.5f);
                lensDistortion.scale.value = Random.Range(0.8f, 1.2f);
            }
            if (chromaticAberration != null) chromaticAberration.intensity.value = Random.Range(0.5f, 1f);

            elapsed += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
        if (lensDistortion != null) lensDistortion.scale.value = 1f;
    }
}