using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticDecay : MonoBehaviour
{
    [Header("References")]
    public Volume postProcessVolume;
    public Rigidbody playerRb;

    [Header("Speed Thresholds")]
    [Tooltip("Speeds below this mean the world starts turning grey")]
    public float minSpeed = 5f;
    [Tooltip("Target speed to achieve maximum neon color")]
    public float maxSpeed = 16f;

    [Header("Visual Limits")]
    public float minSaturation = -100f; // Completely grey/dead
    public float maxSaturation = 25f;   // Over-saturated neon pop
    public float maxChromaticAberration = 0.8f; // RGB edge splitting at high speed

    [Header("Transition Settings")]
    [Tooltip("How smoothly the colors fade in and out. Lower = slower fade.")]
    public float transitionSpeed = 4f;

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        // Safely fetch the specific effect overrides from the Volume Profile
        if (postProcessVolume.profile.TryGet(out ColorAdjustments ca))
            colorAdjustments = ca;

        if (postProcessVolume.profile.TryGet(out ChromaticAberration cb))
            chromaticAberration = cb;
    }

    void Update()
    {
        if (colorAdjustments == null) return;

        // 1. Get our current flat horizontal speed
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float currentSpeed = flatVel.magnitude;

        // 2. Calculate a 0.0 to 1.0 percentage based on our speed
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);

        // 3. Calculate what the values SHOULD be right now based on our speed
        float targetSaturation = Mathf.Lerp(minSaturation, maxSaturation, speedPercent);

        // 4. SMOOTHING: Gradually shift the actual screen saturation towards the target over time
        colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, Time.deltaTime * transitionSpeed);

        // 5. SMOOTHING: Gradually shift the Chromatic Aberration
        if (chromaticAberration != null)
        {
            float targetCA = Mathf.Lerp(0f, maxChromaticAberration, speedPercent);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, targetCA, Time.deltaTime * transitionSpeed);
        }
    }
}