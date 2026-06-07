using UnityEngine;

public class SkyboxEvolver : MonoBehaviour
{
    [Header("References")]
    public Rigidbody playerRb;
    public Material skyboxMaterial; // Drag your lighting skybox material here

    [Header("Speed Thresholds")]
    public float minSpeed = 5f;
    public float maxSpeed = 16f;

    [Header("Skybox Colors (Void to Aurora)")]
    [ColorUsage(true, true)] public Color slowColor = Color.black;
    [ColorUsage(true, true)] public Color fastColor = new Color(0.5f, 1f, 0.5f); // Neon green tint

    [Header("Exposure Transition")]
    public float minExposure = 0.2f;
    public float maxExposure = 1.5f;
    public float transitionSpeed = 3f;

    private float currentExposure;
    private Color currentColor;

    void Start()
    {
        // Initialize starting values
        currentExposure = minExposure;
        currentColor = slowColor;
    }

    void Update()
    {
        if (skyboxMaterial == null) return;

        // 1. Calculate Player Speed Percentage
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, flatVel.magnitude);

        // 2. Determine Target Color and Exposure
        Color targetColor = Color.Lerp(slowColor, fastColor, speedPercent);
        float targetExposure = Mathf.Lerp(minExposure, maxExposure, speedPercent);

        // 3. Smoothly Transition
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * transitionSpeed);

        // 4. Apply to Unity's Skybox Shader
        skyboxMaterial.SetColor("_Tint", currentColor);
        skyboxMaterial.SetFloat("_Exposure", currentExposure);

        // (Optional) Update environment lighting to match
        DynamicGI.UpdateEnvironment();
    }

    void OnApplicationQuit()
    {
        // Reset material on exit so it doesn't stay permanently glowing in the editor
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", Color.gray);
            skyboxMaterial.SetFloat("_Exposure", 1f);
        }
    }
}