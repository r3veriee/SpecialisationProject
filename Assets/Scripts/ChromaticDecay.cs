//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

//public class ChromaticDecay : MonoBehaviour
//{
//    [Header("References")]
//    public Volume postProcessVolume;
//    public Rigidbody playerRb;

//    [Header("Speed Thresholds")]
//    [Tooltip("Speeds below this mean the world starts turning grey")]
//    public float minSpeed = 5f;
//    [Tooltip("Target speed to achieve maximum neon color")]
//    public float maxSpeed = 16f;

//    [Header("Visual Limits")]
//    public float minSaturation = -100f; // Completely grey/dead
//    public float maxSaturation = 25f;   // Over-saturated neon pop
//    public float maxChromaticAberration = 0.8f; // RGB edge splitting at high speed

//    [Header("Transition Settings")]
//    [Tooltip("How smoothly the colors fade in and out. Lower = slower fade.")]
//    public float transitionSpeed = 4f;

//    private ColorAdjustments colorAdjustments;
//    private ChromaticAberration chromaticAberration;
//    private float burstTimer = 0f;
//    void Start()
//    {
//        // Safely fetch the specific effect overrides from the Volume Profile
//        if (postProcessVolume.profile.TryGet(out ColorAdjustments ca))
//            colorAdjustments = ca;

//        if (postProcessVolume.profile.TryGet(out ChromaticAberration cb))
//            chromaticAberration = cb;
//    }

//    void Update()
//    {
//        if (colorAdjustments == null) return;

//        // 1. Get our current flat horizontal speed
//        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
//        float currentSpeed = flatVel.magnitude;

//        // 2. Calculate a 0.0 to 1.0 percentage based on our speed
//        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);

//        float targetSaturation;
//        float targetCA;

//        if (burstTimer > 0)
//        {
//            // If we just grabbed a shard, OVERRIDE to maximum neon!
//            burstTimer -= Time.deltaTime;
//            targetSaturation = maxSaturation;
//            targetCA = maxChromaticAberration;
//        }
//        else
//        {
//            // Normal speed-based logic
//            targetSaturation = Mathf.Lerp(minSaturation, maxSaturation, speedPercent);
//            targetCA = Mathf.Lerp(0f, maxChromaticAberration, speedPercent);
//        }
//    }

//    // Call this to force max saturation for a few seconds
//    public void TriggerColourBurst(float duration)
//    {
//        burstTimer = duration;
//    }
//}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticDecay : MonoBehaviour
{
    public Volume postProcessVolume;
    public Rigidbody playerRb;

    public float minSpeed = 5f;
    public float maxSpeed = 16f;

    public float minSaturation = -100f;
    public float maxSaturation = 25f;
    public float maxChromaticAberration = 0.8f;
    public float transitionSpeed = 4f;

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private float burstTimer = 0f;

    void Start()
    {
        if (postProcessVolume.profile.TryGet(out ColorAdjustments ca)) colorAdjustments = ca;
        if (postProcessVolume.profile.TryGet(out ChromaticAberration cb)) chromaticAberration = cb;
    }

    void Update()
    {
        if (colorAdjustments == null) return;

        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float currentSpeed = flatVel.magnitude;
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);

        float targetSaturation;
        float targetCA;

        if (burstTimer > 0)
        {
            burstTimer -= Time.deltaTime;
            targetSaturation = maxSaturation;
            targetCA = maxChromaticAberration;
        }
        else
        {
            targetSaturation = Mathf.Lerp(minSaturation, maxSaturation, speedPercent);
            targetCA = Mathf.Lerp(0f, maxChromaticAberration, speedPercent);
        }

        colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, Time.deltaTime * transitionSpeed);

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, targetCA, Time.deltaTime * transitionSpeed);
        }
    }

    public void TriggerColourBurst(float duration)
    {
        burstTimer = duration;
    }
}