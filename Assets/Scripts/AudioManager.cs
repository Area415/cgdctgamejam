using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] tells Unity to let us see and edit this custom class in the Inspector
[System.Serializable]
public class AudioEntry
{
    public string name;      // The exact name you will type in Yarn (e.g., "FrisscityCalm")
    public AudioClip clip;   // The actual audio file you drag and drop in the Inspector
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance => instance;

    [Header("Audio Files")]
    // These lists are what you will actually see and fill out in the Unity Inspector
    [SerializeField] private List<AudioEntry> music = new List<AudioEntry>();
    [SerializeField] private List<AudioEntry> sfx = new List<AudioEntry>();

    // Dictionaries are hidden from the Inspector, but they allow the code to 
    // look up your audio files instantly during gameplay without lagging.
    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private AudioSource persistentMusicSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Keep the first persistent manager, but merge any missing entries from
            // scene-local duplicates so later scenes can extend the catalog.
            instance.MergeMissingEntriesFrom(this);
            Destroy(gameObject);
            return;
        }

        persistentMusicSource = GetComponent<AudioSource>();
        if (persistentMusicSource == null)
        {
            persistentMusicSource = gameObject.AddComponent<AudioSource>();
        }
        // Let the Music volume slider control this source.
        if (persistentMusicSource.outputAudioMixerGroup == null)
        {
            persistentMusicSource.outputAudioMixerGroup = GameSettings.MusicGroup;
        }

        // When the game starts, this loop copies all the music from your Inspector List 
        // into the Dictionary for lightning-fast lookups.
        foreach (var entry in music)
        {
            musicDictionary[entry.name] = entry.clip;
        }

        // Does the exact same thing for your Sound Effects
        foreach (var entry in sfx)
        {
            sfxDictionary[entry.name] = entry.clip;
        }
    }

    private void MergeMissingEntriesFrom(AudioManager other)
    {
        if (other == null)
        {
            return;
        }

        foreach (var entry in other.music)
        {
            if (entry == null || string.IsNullOrEmpty(entry.name) || entry.clip == null)
            {
                continue;
            }

            if (!musicDictionary.ContainsKey(entry.name))
            {
                musicDictionary[entry.name] = entry.clip;
            }
        }

        foreach (var entry in other.sfx)
        {
            if (entry == null || string.IsNullOrEmpty(entry.name) || entry.clip == null)
            {
                continue;
            }

            if (!sfxDictionary.ContainsKey(entry.name))
            {
                sfxDictionary[entry.name] = entry.clip;
            }
        }
    }

    // Your VisualNovel.cs script calls this method when it needs a background track
    public AudioClip GetMusic(string name)
    {
        if (musicDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip; // Found it! Send the audio clip back.
        }

        // If you make a typo in Yarn, this warns you exactly which file is missing!
        Debug.LogWarning($"Music '{name}' was not found in the AudioManager.");
        return null;
    }

    // Your VisualNovel.cs script calls this method when it needs a sound effect
    public AudioClip GetSFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip; // Found it! Send the audio clip back.
        }

        Debug.LogWarning($"SFX '{name}' was not found in the AudioManager.");
        return null;
    }

    public void PauseMusic()
    {
        if (persistentMusicSource != null)
        {
            persistentMusicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (persistentMusicSource != null)
        {
            persistentMusicSource.UnPause();
        }
    }
}