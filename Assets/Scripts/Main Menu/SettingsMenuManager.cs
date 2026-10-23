using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;


public class SettingsMenuManager : MonoBehaviour
{
    [Header("Main Contents")]
    public GameObject SettingsContents;
    public GameObject CreditsContents;

    public Button SettingsButton;
    public Button CreditsButton;

    public Sprite SettingsSelected;
    public Sprite SettingsUnselected;

    public Sprite CreditsSelected;
    public Sprite CreditsUnselected;

    public Button exitToTitleButton;


    [Header("Volume Sliders")]
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;
    [Tooltip("No longer used: volume now goes through GameSettings. Kept so existing scene references don't break.")]
    public AudioMixer MainAudioMixer;

    [Header("Text Speed Buttons")]
    public TMP_Dropdown TextSpeedDropdown;

    public Button TSp1Button;
    public Button TSp15Button;
    public Button TSp2Button;

    public Sprite Selected1XButton;
    public Sprite Unselected1XButton;

    public Sprite Selected15XButton;
    public Sprite Unselected15XButton;

    public Sprite Selected2XButton;
    public Sprite Unselected2XButton;

    [Header("Text Size Buttons")]
    public Button SmallButton;
    public Button MidButton;
    public Button BigButton;

    public Sprite SelectedSmallButton;
    public Sprite UnselectedSmallButton;

    public Sprite SelectedMidButton;
    public Sprite UnselectedMidButton;

    public Sprite SelectedBigButton;
    public Sprite UnselectedBigButton;

    private int TextSizeIndex = 1;
    private int TextSpeedIndex = 0;


    private void Awake()
    {
        // Sliders now work in 0..1 (converted to decibels by GameSettings),
        // so the middle of the slider sounds like half volume.
        SetupVolumeSlider(MasterVolumeSlider);
        SetupVolumeSlider(MusicVolumeSlider);
        SetupVolumeSlider(SFXVolumeSlider);
    }

    private static void SetupVolumeSlider(Slider slider)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    // Leaving the menu any other way (e.g. switching cyberdeck tabs) drops unsaved changes.
    private void OnDisable()
    {
        RevertUnsavedChanges();
    }

    private void Start()
    {
        if (CreditsContents != null && SettingsContents != null)
        {
            ShowSettingsContents();
        }
    }

    public void ShowSettingsContents()
    {
        if (CreditsContents != null) CreditsContents.SetActive(false);
        if (SettingsContents != null) SettingsContents.SetActive(true);

        if (SettingsButton != null) SettingsButton.GetComponent<Image>().sprite = SettingsSelected;
        if (CreditsButton != null) CreditsButton.GetComponent<Image>().sprite = CreditsUnselected;
    }

    public void ShowCreditsContents()
    {
        RevertUnsavedChanges();
        
        if (CreditsContents != null) CreditsContents.SetActive(true);
        if (SettingsContents != null) SettingsContents.SetActive(false);

        if (SettingsButton != null) SettingsButton.GetComponent<Image>().sprite = SettingsUnselected;
        if (CreditsButton != null) CreditsButton.GetComponent<Image>().sprite = CreditsSelected;
    }

    //TEXT SIZE
    public void TextSizeSmallPressed()
    {
        if (SmallButton != null) SmallButton.GetComponent<Image>().sprite = SelectedSmallButton;
        if (MidButton != null) MidButton.GetComponent<Image>().sprite = UnselectedMidButton;
        if (BigButton != null) BigButton.GetComponent<Image>().sprite = UnselectedBigButton;

        TextSizeIndex = 0;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }
    public void TextSizeMidPressed()
    {
        if (MidButton != null) MidButton.GetComponent<Image>().sprite = SelectedMidButton;
        if (SmallButton != null) SmallButton.GetComponent<Image>().sprite = UnselectedSmallButton;
        if (BigButton != null) BigButton.GetComponent<Image>().sprite = UnselectedBigButton;

        TextSizeIndex = 1;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }
    public void TextSizeBigPressed()
    {
        if (BigButton != null) BigButton.GetComponent<Image>().sprite = SelectedBigButton;
        if (MidButton != null) MidButton.GetComponent<Image>().sprite = UnselectedMidButton;
        if (SmallButton != null) SmallButton.GetComponent<Image>().sprite = UnselectedSmallButton;

        TextSizeIndex = 2;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }

    //TEXT SPEED
    public void ApplyTextSpeed()
    {
        if (TextSpeedDropdown != null)
        {
            TextSpeedIndex = TextSpeedDropdown.value;
            GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
            GameSettings.Save();
        }
    }

    public void TextSpeed1XPressed()
    {
        if (TSp1Button != null) TSp1Button.GetComponent<Image>().sprite = Selected1XButton;
        if (TSp15Button != null) TSp15Button.GetComponent<Image>().sprite = Unselected15XButton;
        if (TSp2Button != null) TSp2Button.GetComponent<Image>().sprite = Unselected2XButton;

        TextSpeedIndex = 0;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }
    public void TextSpeed15XPressed()
    {
        if (TSp15Button != null) TSp15Button.GetComponent<Image>().sprite = Selected15XButton;
        if (TSp1Button != null) TSp1Button.GetComponent<Image>().sprite = Unselected1XButton;
        if (TSp2Button != null) TSp2Button.GetComponent<Image>().sprite = Unselected2XButton;

        TextSpeedIndex = 1;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }
    public void TextSpeed2XPressed()
    {
        if (TSp2Button != null) TSp2Button.GetComponent<Image>().sprite = Selected2XButton;
        if (TSp1Button != null) TSp1Button.GetComponent<Image>().sprite = Unselected1XButton;
        if (TSp15Button != null) TSp15Button.GetComponent<Image>().sprite = Unselected15XButton;

        TextSpeedIndex = 2;
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
    }

    //VOLUME
    public void ChangeMasterVolume() => PreviewVolumesFromSliders();
    public void ChangeMusicVolume()  => PreviewVolumesFromSliders();
    public void ChangeSFXVolume()    => PreviewVolumesFromSliders();

    // Sends all slider values to GameSettings. A menu without a given slider
    // (e.g. the cyberdeck has no Master slider) keeps that volume as it is.
    private void PreviewVolumesFromSliders()
    {
        GameSettings.PreviewVolumes(
            MasterVolumeSlider != null ? MasterVolumeSlider.value : GameSettings.MasterVolume,
            MusicVolumeSlider  != null ? MusicVolumeSlider.value  : GameSettings.MusicVolume,
            SFXVolumeSlider    != null ? SFXVolumeSlider.value    : GameSettings.SfxVolume);
    }

    // Moves the sliders to match GameSettings without triggering their change events.
    private void SyncVolumeSliders()
    {
        if (MasterVolumeSlider != null) MasterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        if (MusicVolumeSlider  != null) MusicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        if (SFXVolumeSlider    != null) SFXVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
    }

    public void ExitSettingsMenu()
    {
        RevertUnsavedChanges();

        gameObject.SetActive(false);
    }

    public void SaveSettings()
    {
        //Volume
        PreviewVolumesFromSliders();

        //Text Speed + Size (through GameSettings so the dialogue updates too)
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);

        GameSettings.Save();
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        //Volume
        SyncVolumeSliders();

        //Text Speed
        TextSpeedIndex = GameSettings.TextSpeedIndex;
        switch (TextSpeedIndex)
        {
            case 0:
                TextSpeed1XPressed();
                break;
            case 1:
                TextSpeed15XPressed();
                break;
            case 2:
                TextSpeed2XPressed();
                break;
        }

        //Text Size
        TextSizeIndex = GameSettings.TextSizeIndex;
        switch (TextSizeIndex)
        {
            case 0:
                TextSizeSmallPressed();
                break;
            case 1:
                TextSizeMidPressed();
                break;
            case 2:
                TextSizeBigPressed();
                break;
        }

        Debug.Log("Settings loaded!");
    }

    private void RevertUnsavedChanges()
    {
        GameSettings.Revert();

        //Volume
        SyncVolumeSliders();

        //Text Speed
        TextSpeedIndex = GameSettings.TextSpeedIndex;
        switch (TextSpeedIndex)
        {
            case 0:
                TextSpeed1XPressed();
                break;
            case 1:
                TextSpeed15XPressed();
                break;
            case 2:
                TextSpeed2XPressed();
                break;
        }

        //Text Size
        TextSizeIndex = GameSettings.TextSizeIndex;
        switch (TextSizeIndex)
        {
            case 0:
                TextSizeSmallPressed();
                break;
            case 1:
                TextSizeMidPressed();
                break;
            case 2:
                TextSizeBigPressed();
                break;
        }
    }

    public void exitToTitle()
    {
        if (exitToTitleButton != null) SceneManager.LoadScene("MainMenuScene");
    }
}
