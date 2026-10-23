using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Makes the VN dialogue box honor the player's text speed and size settings.
///
/// Put this on the same GameObject as Yarn's LinePresenter
/// (inside the Dynamic_Dialogue_System prefab). It applies the settings when
/// the scene starts and again any time GameSettings.Changed fires, so changes
/// made in the cyberdeck take effect immediately.
/// </summary>
public class DialogueSettingsApplier : MonoBehaviour
{
    [Tooltip("Leave empty to use the LinePresenter on this same GameObject.")]
    [SerializeField] private LinePresenter linePresenter;

    [Tooltip("Font size used for the 'Mid' setting. Small and Big are scaled from this.")]
    [SerializeField] private float baseFontSize = 28f;

    private void Awake()
    {
        if (linePresenter == null) linePresenter = GetComponent<LinePresenter>();
    }

    private void OnEnable()  => GameSettings.Changed += Apply;
    private void OnDisable() => GameSettings.Changed -= Apply;

    // Start runs after every Awake, so LinePresenter has already built its typewriter.
    private void Start() => Apply();

    private void Apply()
    {
        if (linePresenter == null)
        {
            Debug.LogWarning($"{name}: DialogueSettingsApplier can't find a LinePresenter.", this);
            return;
        }

        // Text speed
        if (linePresenter.Typewriter is LetterTypewriter letters)
        {
            letters.CharactersPerSecond = GameSettings.LettersPerSecond;
        }
        else
        {
            Debug.LogWarning($"{name}: LinePresenter's Typewriter Style isn't 'By Letter', so text speed can't be applied.", this);
        }

        // Text size
        if (linePresenter.lineText != null)
        {
            linePresenter.lineText.fontSize = baseFontSize * GameSettings.SizeMultiplier;
        }
    }
}
