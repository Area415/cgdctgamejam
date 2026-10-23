using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;


#nullable enable

public class ChatDialogueViewBubble : MonoBehaviour
{
    [SerializeField] GameObject? typingIndicator;

    private TMP_Text? TextView => GetComponentInChildren<TMP_Text>();

    public bool HasIndicator => typingIndicator != null;

    private void Awake()
    {
        // Follow the player's Text Size setting.
        if (TextView != null) SettingsTextScaler.AddTo(TextView);
    }

    public void ShowTyping()
    {
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(true);
        }
        if (TextView != null)
        {
            TextView.text = string.Empty;
        }
    }

    public void ShowText(string text)
    {
        if (TextView != null)
        {
            TextView.textWrappingMode = TMPro.TextWrappingModes.Normal;
            //TextView.SetTextWrapping(true);
        }
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }
        if (TextView != null)
        {
            TextView.text = text;
        }
    }
}
