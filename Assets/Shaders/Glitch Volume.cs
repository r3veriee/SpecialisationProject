using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/Glitch")]
public class GlitchVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter scanlineJitter = new ClampedFloatParameter(10f, 0f, 50f);
    public ClampedFloatParameter colorDrift = new ClampedFloatParameter(1f, 0f, 5f);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => true;
}