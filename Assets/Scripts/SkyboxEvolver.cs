using UnityEngine;

public class SkyboxEvolver : MonoBehaviour
{
    public Rigidbody playerRb;

    [Tooltip("Drag the material you put on the Giant Sphere in here!")]
    public Material sphereMaterial;

    public float minSpeed = 5f;
    public float maxSpeed = 16f;

    [ColorUsage(true, true)] public Color slowColor = Color.black;
    [ColorUsage(true, true)] public Color fastColor = new Color(0.5f, 1f, 0.5f);

    public float minExposure = 0.2f;
    public float maxExposure = 1.5f;
    public float transitionSpeed = 3f;

    private float currentExposure;
    private Color currentColor;

    void Start()
    {
        currentExposure = minExposure;
        currentColor = slowColor;
    }

    void Update()
    {
        if (sphereMaterial == null || playerRb == null) return;

        // Calculate Player Speed
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, flatVel.magnitude);

        // Figure out the target color and target brightness
        Color targetColor = Color.Lerp(slowColor, fastColor, speedPercent);
        float targetExposure = Mathf.Lerp(minExposure, maxExposure, speedPercent);

        // Smoothly transition to those targets
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * transitionSpeed);

        // Combine them
        Color finalGlowColor = currentColor * currentExposure;

        // Send it to the Giant Sphere
        if (sphereMaterial.HasProperty("_Color"))
            sphereMaterial.SetColor("_Color", finalGlowColor);

        if (sphereMaterial.HasProperty("_BaseColor"))
            sphereMaterial.SetColor("_BaseColor", finalGlowColor);
    }
}