using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingsController : MonoBehaviour
{
    public AudioMixer mainMixer;   // drag MainMixer asset here
    public Slider musicSlider;     // drag your music slider here
    public Slider sfxSlider;       // drag your sfx slider here

    void Start()
    {
        // Load saved values, default to 75% if none saved yet
        float savedMusic = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("Settings_SFXVolume", 0.75f);

        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float sliderValue)
    {
        mainMixer.SetFloat("MusicVolume", LinearToDecibel(sliderValue));
        PlayerPrefs.SetFloat("Settings_MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat("SFXVolume", LinearToDecibel(sliderValue));
        PlayerPrefs.SetFloat("Settings_SFXVolume", sliderValue);
    }

    private float LinearToDecibel(float sliderValue)
    {
        if (sliderValue <= 0.0001f) return -80f; // essentially silent
        return Mathf.Log10(sliderValue) * 20f;
    }
}