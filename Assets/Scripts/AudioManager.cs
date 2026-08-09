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

    [Header("Glitch-Linked Distortion")]
    public AudioMixer mainMixer;
    public GlitchEffect glitchScript;
    [Tooltip("Multiplies glitch intensity before sending to audio distortion")]
    public float distortionResponseCurve = 1f;

    public void RegisterGlitchEffect(GlitchEffect glitch)
    {
        glitchScript = glitch;
    }
    public void UnregisterGlitchEffect(GlitchEffect glitch)
    {
        if (glitchScript == glitch)
        {
            glitchScript = null;
            mainMixer.SetFloat("SFXDistortion", 0f); // reset distortion so it doesn't get stuck on
        }
    }

    void Update()
    {
        if (glitchScript == null || mainMixer == null) return; // safe no-op while in Main Menu or any scene without a glitch effect

        float distortion = Mathf.Clamp01(glitchScript.CurrentIntensity01 * distortionResponseCurve);
        mainMixer.SetFloat("SFXDistortion", distortion);
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