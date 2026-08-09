using UnityEngine;

public class LevelMusicStarter : MonoBehaviour
{
    public AudioClip musicClip;

    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(musicClip);
    }
}