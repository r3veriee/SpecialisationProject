using UnityEngine;
using UnityEngine.Rendering;

public class GlitchEffect : MonoBehaviour
{
    [Header("References")]
    public Volume postProcessVolume;
    public Rigidbody playerRb;

    [Header("Speed Thresholds")]
    public float minSpeed = 2f;
    public float maxSpeed = 15f;

    [Header("Maximum Glitch Settings")]
    public float maxIntensity = 1f;
    public float maxScanlineJitter = 25f;
    public float maxColorDrift = 3f;

    [Header("Smoothing")]
    public float transitionSpeed = 8f;

    private GlitchVolume glitchVolume;
    private bool isPermanentlyDisabled = false;

    void Start()
    {
        if (postProcessVolume.profile.TryGet(out GlitchVolume gv))
        {
            glitchVolume = gv;
            glitchVolume.intensity.overrideState = true;
            glitchVolume.scanlineJitter.overrideState = true;
            glitchVolume.colorDrift.overrideState = true;
        }
    }

    void Update()
    {
        if (isPermanentlyDisabled || glitchVolume == null || playerRb == null) return;

        //Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        //float currentSpeed = flatVel.magnitude;
        float currentSpeed = playerRb.linearVelocity.magnitude;
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
        float glitchMultiplier = 1f - speedPercent;

        float targetIntensity = maxIntensity * glitchMultiplier;
        float targetJitter = maxScanlineJitter * glitchMultiplier;
        float targetDrift = maxColorDrift * glitchMultiplier;

        glitchVolume.intensity.value = Mathf.Lerp(glitchVolume.intensity.value, targetIntensity, Time.deltaTime * transitionSpeed);
        glitchVolume.scanlineJitter.value = Mathf.Lerp(glitchVolume.scanlineJitter.value, targetJitter, Time.deltaTime * transitionSpeed);
        glitchVolume.colorDrift.value = Mathf.Lerp(glitchVolume.colorDrift.value, targetDrift, Time.deltaTime * transitionSpeed);
    }

    public void DisableEffect()
    {
        isPermanentlyDisabled = true;
        if (glitchVolume != null)
        {
            glitchVolume.intensity.value = 0f;
            glitchVolume.scanlineJitter.value = 0f;
            glitchVolume.colorDrift.value = 0f;
        }
    }
}