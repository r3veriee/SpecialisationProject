using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    [Header("Sliders")]
    public Slider glitchSlider;
    public Slider emissionSlider;

    [Header("Glitch Settings")]
    public GlitchEffect glitchScript;
    public ChromaticDecay chromaticScript;
    [Tooltip("The glitch effect can never go below this scale, even at slider = 0")]
    [Range(0f, 1f)] public float minGlitchScale = 0.25f;
    [Tooltip("The glitch effect's scale at slider = 1")]
    [Range(0f, 1f)] public float maxGlitchScale = 1f;

    [Header("Selected Materials (Emission Reduction)")]
    [Tooltip("Only materials in this list get their emission dimmed. Everything else is untouched.")]
    public Material[] emissiveMaterialsToControl;
    [Tooltip("Lowest emission multiplier the slider can reach (0 = fully off, 0.2 = still visibly glowing)")]
    public float maxEmissionIntensity = 3f;
    public float minEmissionIntensity = 0.1f;

    private string colorPropertyName = "_EmissionColor";
    private Color[] originalEmissionColors;
    private Color[] emissionHues;

    private const string GlitchPrefKey = "Accessibility_GlitchScale";
    private const string EmissionPrefKey = "Accessibility_EmissionScale";

    void Start()
    {
        CacheOriginalEmissionColors();

        float savedGlitch = PlayerPrefs.GetFloat(GlitchPrefKey, 1f);
        float savedEmission = PlayerPrefs.GetFloat(EmissionPrefKey, 1f);

        if (glitchSlider != null)
        {
            glitchSlider.SetValueWithoutNotify(savedGlitch);
            UpdateGlitch(savedGlitch);
            glitchSlider.onValueChanged.AddListener(OnGlitchSliderChanged);
        }

        if (emissionSlider != null && emissiveMaterialsToControl.Length > 0)
        {
            emissionSlider.SetValueWithoutNotify(savedEmission);
            UpdateEmission(savedEmission);
            emissionSlider.onValueChanged.AddListener(OnEmissionSliderChanged);
        }
    }

    private void CacheOriginalEmissionColors()
    {
        emissionHues = new Color[emissiveMaterialsToControl.Length];
        for (int i = 0; i < emissiveMaterialsToControl.Length; i++)
        {
            if (emissiveMaterialsToControl[i] == null) continue;
            Color raw = emissiveMaterialsToControl[i].GetColor(colorPropertyName);
            float maxChannel = Mathf.Max(raw.r, raw.g, raw.b, 0.0001f);
            emissionHues[i] = raw / maxChannel; // pure color, magnitude ~1
        }
    }

    public void UpdateEmission(float sliderValue)
    {
        float intensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, sliderValue);
        for (int i = 0; i < emissiveMaterialsToControl.Length; i++)
        {
            if (emissiveMaterialsToControl[i] == null) continue;
            emissiveMaterialsToControl[i].SetColor(colorPropertyName, emissionHues[i] * intensity);
        }
    }

    private void OnGlitchSliderChanged(float value)
    {
        UpdateGlitch(value);
        PlayerPrefs.SetFloat(GlitchPrefKey, value);
    }

    private void OnEmissionSliderChanged(float value)
    {
        UpdateEmission(value);
        PlayerPrefs.SetFloat(EmissionPrefKey, value);
    }

    public void UpdateGlitch(float sliderValue)
    {
        // Remaps slider 0-1 onto [minGlitchScale, maxGlitchScale] so it never fully disables
        float scale = Mathf.Lerp(minGlitchScale, maxGlitchScale, sliderValue);
        if (glitchScript != null) glitchScript.accessibilityScale = scale;
        if (chromaticScript != null) chromaticScript.accessibilityScale = scale;
    }

    public void RefreshMaterialList(Material[] newList)
    {
        emissiveMaterialsToControl = newList;
        CacheOriginalEmissionColors();
        if (emissionSlider != null)
            UpdateEmission(emissionSlider.value);
    }

    void OnApplicationQuit()
    {
        for (int i = 0; i < emissiveMaterialsToControl.Length; i++)
        {
            if (emissiveMaterialsToControl[i] != null)
                emissiveMaterialsToControl[i].SetColor(colorPropertyName, originalEmissionColors[i]);
        }
    }
}