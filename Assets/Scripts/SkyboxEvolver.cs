using UnityEngine;

public class SkyboxEvolver : MonoBehaviour
{
    public Rigidbody playerRb;
    public Material skyboxMaterial;

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
        if (skyboxMaterial == null) return;

        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, flatVel.magnitude);

        Color targetColor = Color.Lerp(slowColor, fastColor, speedPercent);
        float targetExposure = Mathf.Lerp(minExposure, maxExposure, speedPercent);

        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * transitionSpeed);

        skyboxMaterial.SetColor("_Tint", currentColor);
        skyboxMaterial.SetFloat("_Exposure", currentExposure);
    }

    void OnApplicationQuit()
    {
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", Color.gray);
            skyboxMaterial.SetFloat("_Exposure", 1f);
        }
    }
}