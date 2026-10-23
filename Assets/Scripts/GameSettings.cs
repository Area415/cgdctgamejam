using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// The single source of truth for player settings: text speed, text size, and volume.
///
/// Settings menus (main menu AND cyberdeck) WRITE through this class.
/// Anything that displays text (VN dialogue box, cyberdeck chat, history)
/// READS from it and listens to <see cref="Changed"/> so it can update live.
/// Volume is pushed straight to the AudioMixer (Assets/Resources/AudioMixer.mixer).
///
/// Flow:
///   Preview(...) / PreviewVolumes(...) -> apply right away (not saved yet)
///   Save()                             -> write current values to PlayerPrefs
///   Revert()                           -> drop unsaved changes, reload saved values
/// </summary>
public static class GameSettings
{
    // ---- PlayerPrefs keys --------------------------------------------------
    // Text keys match what SettingsMenuManager always used, so saved values carry over.
    private const string SpeedKey = "TextSpeedIndex";
    private const string SizeKey  = "TextSizeIndex";
    // NEW volume keys. The old "MasterVolume"/"MusicVolume"/"SFXVolume" keys stored
    // decibels (-80..0); these store 0..1, so they get new names to avoid misreading old saves.
    private const string MasterKey = "MasterVolume01";
    private const string MusicKey  = "MusicVolume01";
    private const string SfxKey    = "SFXVolume01";

    // ---- Tuning: change these numbers to adjust how each option feels -------

    // Typewriter speed in characters per second for 1X / 1.5X / 2X.
    private static readonly int[]   LettersPerSecondByIndex = { 40, 60, 80 };

    // How much faster each option is relative to 1X (used for cyberdeck chat timing).
    private static readonly float[] SpeedMultiplierByIndex  = { 1f, 1.5f, 2f };

    // Text size as a multiple of each text's normal size, for Small / Mid / Big.
    private static readonly float[] SizeMultiplierByIndex   = { 0.85f, 1f, 1.2f };

    private const int   DefaultSpeedIndex = 0;    // 1X
    private const int   DefaultSizeIndex  = 1;    // Mid
    private const float DefaultMaster     = 1f;   // 100%
    private const float DefaultMusic      = 0.8f; // 80%
    private const float DefaultSfx        = 0.8f; // 80%

    // Names of the exposed parameters / groups in AudioMixer.mixer.
    private const string MixerResourceName = "AudioMixer";
    private const string MasterParam = "MasterVolume";
    private const string MusicParam  = "MusicVolume";
    private const string SfxParam    = "SFXVolume";

    // -----------------------------------------------------------------------

    /// <summary>Fired whenever text speed or size changes (preview or revert).</summary>
    public static event Action Changed;

    public static int TextSpeedIndex { get; private set; }
    public static int TextSizeIndex  { get; private set; }

    public static int   LettersPerSecond => LettersPerSecondByIndex[TextSpeedIndex];
    public static float SpeedMultiplier  => SpeedMultiplierByIndex[TextSpeedIndex];
    public static float SizeMultiplier   => SizeMultiplierByIndex[TextSizeIndex];

    /// <summary>Volumes as 0..1 slider values (0 = silent, 1 = full).</summary>
    public static float MasterVolume { get; private set; }
    public static float MusicVolume  { get; private set; }
    public static float SfxVolume    { get; private set; }

    private static AudioMixer mixer;
    /// <summary>The game's main AudioMixer, loaded from Resources.</summary>
    public static AudioMixer Mixer
    {
        get
        {
            if (mixer == null) mixer = Resources.Load<AudioMixer>(MixerResourceName);
            return mixer;
        }
    }

    /// <summary>Route an AudioSource here to have the Music slider control it.</summary>
    public static AudioMixerGroup MusicGroup => FindGroup("Music");
    /// <summary>Route an AudioSource here to have the SFX slider control it.</summary>
    public static AudioMixerGroup SfxGroup   => FindGroup("SFX");

    // Runs once when the game starts (also on every Play in the editor),
    // so values are loaded before any scene object asks for them.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Changed = null; // clear listeners left over from a previous Play session
        mixer = null;
        LoadSaved();
    }

    // The AudioMixer ignores SetFloat calls made too early in startup, so a tiny
    // helper object applies the saved volumes on its first Start().
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateStartupApplier()
    {
        var go = new GameObject("[GameSettings]");
        go.hideFlags = HideFlags.HideInHierarchy;
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<StartupApplier>();
    }

    private class StartupApplier : MonoBehaviour
    {
        private void Start() => ApplyVolumes();
    }

    /// <summary>Apply new text settings right away without saving them.</summary>
    public static void Preview(int speedIndex, int sizeIndex)
    {
        TextSpeedIndex = Mathf.Clamp(speedIndex, 0, LettersPerSecondByIndex.Length - 1);
        TextSizeIndex  = Mathf.Clamp(sizeIndex,  0, SizeMultiplierByIndex.Length - 1);
        Changed?.Invoke();
    }

    /// <summary>Apply new volumes (0..1) right away without saving them.</summary>
    public static void PreviewVolumes(float master, float music, float sfx)
    {
        MasterVolume = Mathf.Clamp01(master);
        MusicVolume  = Mathf.Clamp01(music);
        SfxVolume    = Mathf.Clamp01(sfx);
        ApplyVolumes();
    }

    /// <summary>Keep the current values for next time.</summary>
    public static void Save()
    {
        PlayerPrefs.SetInt(SpeedKey, TextSpeedIndex);
        PlayerPrefs.SetInt(SizeKey,  TextSizeIndex);
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicKey,  MusicVolume);
        PlayerPrefs.SetFloat(SfxKey,    SfxVolume);
        PlayerPrefs.Save();
    }

    /// <summary>Throw away unsaved changes and go back to the saved values.</summary>
    public static void Revert()
    {
        LoadSaved();
        ApplyVolumes();
        Changed?.Invoke();
    }

    private static void LoadSaved()
    {
        TextSpeedIndex = Mathf.Clamp(PlayerPrefs.GetInt(SpeedKey, DefaultSpeedIndex), 0, LettersPerSecondByIndex.Length - 1);
        TextSizeIndex  = Mathf.Clamp(PlayerPrefs.GetInt(SizeKey,  DefaultSizeIndex),  0, SizeMultiplierByIndex.Length - 1);
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterKey, DefaultMaster));
        MusicVolume  = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey,  DefaultMusic));
        SfxVolume    = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey,    DefaultSfx));
    }

    private static void ApplyVolumes()
    {
        if (Mixer == null)
        {
            Debug.LogWarning("GameSettings: couldn't load AudioMixer from Resources, so volume can't be applied.");
            return;
        }
        Mixer.SetFloat(MasterParam, ToDecibels(MasterVolume));
        Mixer.SetFloat(MusicParam,  ToDecibels(MusicVolume));
        Mixer.SetFloat(SfxParam,    ToDecibels(SfxVolume));
    }

    // Ears hear loudness logarithmically, so a linear 0..1 slider has to be
    // converted to decibels: 1 -> 0 dB (full), 0.5 -> -6 dB, 0.1 -> -20 dB, 0 -> -80 dB (silent).
    private static float ToDecibels(float volume01)
    {
        return volume01 <= 0.0001f ? -80f : Mathf.Log10(volume01) * 20f;
    }

    private static AudioMixerGroup FindGroup(string groupName)
    {
        if (Mixer == null) return null;
        var groups = Mixer.FindMatchingGroups(groupName);
        return groups.Length > 0 ? groups[0] : null;
    }
}
