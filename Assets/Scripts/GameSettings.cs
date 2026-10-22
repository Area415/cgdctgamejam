using System;
using UnityEngine;

/// <summary>
/// The single source of truth for player text settings (speed + size).
///
/// Settings menus (main menu AND cyberdeck) WRITE through this class.
/// Anything that displays text (VN dialogue box, cyberdeck chat, history)
/// READS from it and listens to <see cref="Changed"/> so it can update live.
///
/// Flow:
///   Preview(...)  -> change values + tell everyone (not saved yet)
///   Save()        -> write current values to PlayerPrefs
///   Revert()      -> throw away unsaved changes, reload saved values, tell everyone
/// </summary>
public static class GameSettings
{
    // PlayerPrefs keys. These match the keys SettingsMenuManager already uses,
    // so any settings players have already saved carry over.
    private const string SpeedKey = "TextSpeedIndex";
    private const string SizeKey  = "TextSizeIndex";

    // ---- Tuning: change these numbers to adjust how each option feels ----

    // Typewriter speed in characters per second for 1X / 1.5X / 2X.
    private static readonly int[]   LettersPerSecondByIndex = { 40, 60, 80 };

    // How much faster each option is relative to 1X (used for cyberdeck chat timing).
    private static readonly float[] SpeedMultiplierByIndex  = { 1f, 1.5f, 2f };

    // Text size as a multiple of each text's normal size, for Small / Mid / Big.
    private static readonly float[] SizeMultiplierByIndex   = { 0.85f, 1f, 1.2f };

    private const int DefaultSpeedIndex = 0; // 1X
    private const int DefaultSizeIndex  = 1; // Mid

    // -----------------------------------------------------------------------

    /// <summary>Fired whenever speed or size changes (preview or revert).</summary>
    public static event Action Changed;

    public static int TextSpeedIndex { get; private set; }
    public static int TextSizeIndex  { get; private set; }

    public static int   LettersPerSecond => LettersPerSecondByIndex[TextSpeedIndex];
    public static float SpeedMultiplier  => SpeedMultiplierByIndex[TextSpeedIndex];
    public static float SizeMultiplier   => SizeMultiplierByIndex[TextSizeIndex];

    // Runs once when the game starts (also on every Play in the editor),
    // so values are always loaded before any scene object asks for them.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Changed = null; // clear listeners left over from a previous Play session
        LoadSaved();
    }

    /// <summary>Apply new values right away without saving them.</summary>
    public static void Preview(int speedIndex, int sizeIndex)
    {
        TextSpeedIndex = Mathf.Clamp(speedIndex, 0, LettersPerSecondByIndex.Length - 1);
        TextSizeIndex  = Mathf.Clamp(sizeIndex,  0, SizeMultiplierByIndex.Length - 1);
        Changed?.Invoke();
    }

    /// <summary>Keep the current values for next time.</summary>
    public static void Save()
    {
        PlayerPrefs.SetInt(SpeedKey, TextSpeedIndex);
        PlayerPrefs.SetInt(SizeKey,  TextSizeIndex);
        PlayerPrefs.Save();
    }

    /// <summary>Throw away unsaved changes and go back to the saved values.</summary>
    public static void Revert()
    {
        LoadSaved();
        Changed?.Invoke();
    }

    private static void LoadSaved()
    {
        TextSpeedIndex = Mathf.Clamp(PlayerPrefs.GetInt(SpeedKey, DefaultSpeedIndex), 0, LettersPerSecondByIndex.Length - 1);
        TextSizeIndex  = Mathf.Clamp(PlayerPrefs.GetInt(SizeKey,  DefaultSizeIndex),  0, SizeMultiplierByIndex.Length - 1);
    }
}
