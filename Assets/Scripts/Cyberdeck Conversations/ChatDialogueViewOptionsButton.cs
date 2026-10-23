using System;
using UnityEngine;
using TMPro;
using Yarn.Unity;


#nullable enable

public class ChatDialogueViewOptionsButton : MonoBehaviour
{

    private TMP_Text? TextView => GetComponentInChildren<TMP_Text>();

    private void Awake()
    {
        // Follow the player's Text Size setting.
        if (TextView != null) SettingsTextScaler.AddTo(TextView);
    }

    public string Text
    {
        get => (TextView != null) ? TextView.text : string.Empty;
        set { if (TextView != null) { TextView.text = value; } }
    }

    public Action? OnClick { get; internal set; }

    public void OnClicked()
    {
        OnClick?.Invoke();
    }
}
