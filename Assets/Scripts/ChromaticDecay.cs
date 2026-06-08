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

    private float currentMinSaturation;

    void Start()
    {
        if (postProcessVolume.profile.TryGet(out ColorAdjustments ca)) colorAdjustments = ca;
        if (postProcessVolume.profile.TryGet(out ChromaticAberration cb)) chromaticAberration = cb;
        currentMinSaturation = minSaturation;
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
            targetSaturation = Mathf.Lerp(currentMinSaturation, maxSaturation, speedPercent);
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

    public void ReduceDecayPotency(float buffAmount)
    {
        // Calculate how much saturation to give back based on the shard's percentage
        float totalSaturationRange = maxSaturation - minSaturation;
        float saturationBoost = totalSaturationRange * buffAmount;

        currentMinSaturation += saturationBoost;

        // Cap it so the player can never become completely immune to the mechanic
        if (currentMinSaturation > maxSaturation - 10f)
        {
            currentMinSaturation = maxSaturation - 10f;
        }
    }

    public void RestoreFullColor()
    {
        TriggerColourBurst(2f);
        if (colorAdjustments != null) colorAdjustments.saturation.value = maxSaturation;
        if (chromaticAberration != null) chromaticAberration.intensity.value = maxChromaticAberration;
    }
}