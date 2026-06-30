using UnityEngine;
using UnityEngine.Rendering;

public class GlitchEffect : MonoBehaviour
{
    [Header("References")]
    public Volume postProcessVolume;
    [Tooltip("Drag your Player in here so we can track their speed!")]
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

    void Start()
    {
        if (postProcessVolume.profile.TryGet(out GlitchVolume gv))
        {
            glitchVolume = gv;

            glitchVolume.intensity.overrideState = true;
            glitchVolume.scanlineJitter.overrideState = true;
            glitchVolume.colorDrift.overrideState = true;
        }
        else
        {
            Debug.LogError("CRITICAL: No GlitchVolume found on the Post Process Volume Profile!");
        }
    }

    void Update()
    {
        if (glitchVolume == null || playerRb == null) return;

        // Calculate the player's flat horizontal speed
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float currentSpeed = flatVel.magnitude;

        // Inverse Math:
        // If speed >= maxSpeed, multiplier is 0 (Screen is clear)
        // If speed <= minSpeed, multiplier is 1 (Screen is violently glitching)
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
        float glitchMultiplier = 1f - speedPercent;

        // Calculate target values based on the multiplier
        float targetIntensity = maxIntensity * glitchMultiplier;
        float targetJitter = maxScanlineJitter * glitchMultiplier;
        float targetDrift = maxColorDrift * glitchMultiplier;

        // Smoothly apply the values to the Volume
        glitchVolume.intensity.value = Mathf.Lerp(glitchVolume.intensity.value, targetIntensity, Time.deltaTime * transitionSpeed);
        glitchVolume.scanlineJitter.value = Mathf.Lerp(glitchVolume.scanlineJitter.value, targetJitter, Time.deltaTime * transitionSpeed);
        glitchVolume.colorDrift.value = Mathf.Lerp(glitchVolume.colorDrift.value, targetDrift, Time.deltaTime * transitionSpeed);
    }
}