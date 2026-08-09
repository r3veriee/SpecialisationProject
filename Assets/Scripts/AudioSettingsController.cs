using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    public Slider musicSlider;     // drag your music slider here
    public Slider sfxSlider;       // drag your sfx slider here

    void Start()
    {
        // Load saved values, default to 75% if none saved yet
        float savedMusic = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("Settings_SFXVolume", 0.75f);

        musicSlider.SetValueWithoutNotify(savedMusic);
        sfxSlider.SetValueWithoutNotify(savedSFX);

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float sliderValue)
    {
        AudioManager.Instance.SetBaseMusicVolume(sliderValue);
        PlayerPrefs.SetFloat("Settings_MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        AudioManager.Instance.SetBaseSFXVolume(sliderValue);
        PlayerPrefs.SetFloat("Settings_SFXVolume", sliderValue);
    }
}