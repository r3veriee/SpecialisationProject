using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Setup")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public SoundLibrary library;

    [Header("Pool")]
    public int poolSize = 12;

    private Queue<AudioSource> pool;
    private AudioSource musicSource;
    private Dictionary<SFXType, AudioSource> activeLoops = new Dictionary<SFXType, AudioSource>();

    [Header("Glitch-Linked Distortion & Ducking")]
    public AudioMixer mainMixer;
    private GlitchEffect glitchScript;
    [Tooltip("Multiplies glitch intensity before sending to audio distortion")]
    public float distortionResponseCurve = 1f;
    [Tooltip("Separate response curve for music distortion — usually lower than SFX")]
    public float musicDistortionResponseCurve = 0.4f;

    [Header("Glitch Volume Ducking")]
    [Tooltip("How low SFX volume can duck at max glitch intensity (0.4 = down to 40%)")]
    [Range(0f, 1f)] public float sfxDuckFloor = 0.4f;
    [Tooltip("How low music volume can duck at max glitch intensity — usually gentler than SFX")]
    [Range(0f, 1f)] public float musicDuckFloor = 0.7f;

    [Header("Lowpass (the 'deep/bassy/muffled' feel)")]
    [Tooltip("Cutoff frequency at zero glitch — clean, full range")]
    public float lowpassCleanCutoff = 22000f;
    [Tooltip("Cutoff frequency at max glitch — how deep/muffled it gets")]
    public float lowpassDeepCutoff = 400f;
    [Tooltip("Music can have its own, usually less extreme, deep cutoff")]
    public float musicLowpassDeepCutoff = 800f;

    // Base volumes come from the settings sliders (0-1), saved/loaded via PlayerPrefs by AudioSettingsController
    private float baseMusicVolume = 0.75f;
    private float baseSFXVolume = 0.75f;

    public void RegisterGlitchEffect(GlitchEffect glitch)
    {
        glitchScript = glitch;
    }

    public void UnregisterGlitchEffect(GlitchEffect glitch)
    {
        if (glitchScript == glitch)
        {
            glitchScript = null;
            if (mainMixer != null)
            {
                mainMixer.SetFloat("SFXDistortion", 0f);
                mainMixer.SetFloat("MusicDistortion", 0f);
                mainMixer.SetFloat("SFXLowpassCutoff", lowpassCleanCutoff);
                mainMixer.SetFloat("MusicLowpassCutoff", lowpassCleanCutoff);
                mainMixer.SetFloat("SFXVolume", LinearToDecibel(baseSFXVolume));
                mainMixer.SetFloat("MusicVolume", LinearToDecibel(baseMusicVolume));
            }
        }
    }

    // Called by AudioSettingsController whenever the player moves a slider
    public void SetBaseMusicVolume(float linear01)
    {
        baseMusicVolume = linear01;
    }

    public void SetBaseSFXVolume(float linear01)
    {
        baseSFXVolume = linear01;
    }

    void Update()
    {
        if (mainMixer == null) return;

        float rawGlitch = glitchScript != null ? glitchScript.CurrentIntensity01 : 0f;

        // Push the curve harder so mid-range glitch still reads as noticeable
        // (square root makes low values ramp up faster than linear)
        float glitchAmount = Mathf.Sqrt(Mathf.Clamp01(rawGlitch));

        // Ducking
        float sfxDuckMultiplier = Mathf.Lerp(1f, sfxDuckFloor, glitchAmount);
        float musicDuckMultiplier = Mathf.Lerp(1f, musicDuckFloor, glitchAmount);
        mainMixer.SetFloat("SFXVolume", LinearToDecibel(baseSFXVolume * sfxDuckMultiplier));
        mainMixer.SetFloat("MusicVolume", LinearToDecibel(baseMusicVolume * musicDuckMultiplier));

        // Distortion
        mainMixer.SetFloat("SFXDistortion", Mathf.Clamp01(glitchAmount * distortionResponseCurve));
        mainMixer.SetFloat("MusicDistortion", Mathf.Clamp01(glitchAmount * musicDistortionResponseCurve));

        // Lowpass — the deep/bassy/muffled effect. Note: inverted, low value = more effect
        float sfxCutoff = Mathf.Lerp(lowpassCleanCutoff, lowpassDeepCutoff, glitchAmount);
        float musicCutoff = Mathf.Lerp(lowpassCleanCutoff, musicLowpassDeepCutoff, glitchAmount);
        mainMixer.SetFloat("SFXLowpassCutoff", sfxCutoff);
        mainMixer.SetFloat("MusicLowpassCutoff", musicCutoff);
    }

    private float LinearToDecibel(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(linear) * 20f;
    }

    void Awake()
    {
        // Enforce singleton: if one already exists, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // survives scene loads

        // Build a pool of reusable AudioSources for one-shot SFX
        pool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.playOnAwake = false;
            pool.Enqueue(src);
        }

        // One dedicated source just for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    public void PlaySFX(SFXType type)
    {
        SoundLibrary.SFXEntry entry = library.Get(type);
        if (entry == null || entry.clips.Length == 0) return;

        AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];

        if (entry.loop)
        {
            PlayLooping(type, entry, clip);
            return;
        }

        AudioSource src = pool.Dequeue();
        pool.Enqueue(src); // send it to the back so we cycle through all of them
        src.Stop();
        src.clip = clip;
        src.volume = entry.volume;
        src.loop = false;
        src.Play();
    }

    private void PlayLooping(SFXType type, SoundLibrary.SFXEntry entry, AudioClip clip)
    {
        if (activeLoops.ContainsKey(type) && activeLoops[type].isPlaying) return;

        AudioSource src = pool.Dequeue();
        pool.Enqueue(src);
        src.Stop();
        src.clip = clip;
        src.volume = entry.volume;
        src.loop = true;
        src.Play();
        activeLoops[type] = src;
    }

    public void StopLoopingSFX(SFXType type)
    {
        if (activeLoops.TryGetValue(type, out AudioSource src))
        {
            src.Stop();
            activeLoops.Remove(type);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        // If this exact clip is already playing, don't restart it — let it keep going
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }
}