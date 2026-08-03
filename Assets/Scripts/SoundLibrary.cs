using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    public SFXEntry[] entries;

    public SFXEntry Get(SFXType type)
    {
        foreach (var e in entries)
        {
            if (e.type == type) return e;
        }
        Debug.LogWarning("No SFX entry found for " + type);
        return null;
    }
}