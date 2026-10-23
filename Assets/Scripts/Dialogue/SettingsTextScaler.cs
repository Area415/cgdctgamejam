using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scales a TextMeshPro text by the player's Text Size setting (GameSettings.SizeMultiplier).
///
/// It remembers the size the text was designed at (whatever is set in the Inspector)
/// and multiplies from that, so Small/Mid/Big always look right relative to the design.
/// Updates live when the setting changes, and tells the layout to re-measure so
/// bubbles and history rows grow or shrink to fit.
///
/// Usually added automatically by code (chat bubbles, options, history lines),
/// but you can also drop it on any TMP text in the Inspector.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class SettingsTextScaler : MonoBehaviour
{
    private TMP_Text text;
    private bool captured;
    private float baseSize, baseMin, baseMax;

    /// <summary>Adds a scaler to this text if it doesn't already have one.</summary>
    public static SettingsTextScaler AddTo(TMP_Text target)
    {
        if (target == null) return null;
        var scaler = target.GetComponent<SettingsTextScaler>();
        return scaler != null ? scaler : target.gameObject.AddComponent<SettingsTextScaler>();
    }

    private void Awake() => Capture();

    private void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    private void OnDisable() => GameSettings.Changed -= Apply;

    private void Capture()
    {
        if (captured) return;
        text = GetComponent<TMP_Text>();
        baseSize = text.fontSize;
        baseMin  = text.fontSizeMin;
        baseMax  = text.fontSizeMax;
        captured = true;
    }

    private void Apply()
    {
        Capture();
        float m = GameSettings.SizeMultiplier;

        if (text.enableAutoSizing)
        {
            // Auto Size ignores fontSize, so scale its min/max range instead.
            text.fontSizeMin = baseMin * m;
            text.fontSizeMax = baseMax * m;
        }
        else
        {
            text.fontSize = baseSize * m;
        }

        // Ask the surrounding layout (bubble, list) to re-measure at the new size.
        LayoutRebuilder.MarkLayoutForRebuild(text.rectTransform);
    }
}
