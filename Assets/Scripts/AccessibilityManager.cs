using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    [Header("Sliders")]
    public Slider glitchSlider;
    public Slider brightnessSlider;

    [Header("References")]
    public GlitchEffect glitchScript;
    public ChromaticDecay chromaticScript;

    [Header("Material Brightness")]
    public Material targetMaterial;
    public string colorPropertyName = "_Color";

    private Color originalColor;

    void Start()
    {
        if (glitchSlider != null)
        {
            glitchSlider.onValueChanged.AddListener(UpdateGlitch);
            glitchSlider.value = 1f;
        }

        if (brightnessSlider != null && targetMaterial != null)
        {
            originalColor = targetMaterial.GetColor(colorPropertyName);
            brightnessSlider.onValueChanged.AddListener(UpdateBrightness);
            brightnessSlider.value = 1f;
        }
    }

    public void UpdateGlitch(float value)
    {
        if (glitchScript != null) glitchScript.accessibilityScale = value;
        if (chromaticScript != null) chromaticScript.accessibilityScale = value;
    }

    public void UpdateBrightness(float value)
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetColor(colorPropertyName, originalColor * value);
        }
    }

    void OnApplicationQuit()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetColor(colorPropertyName, originalColor);
        }
    }
}