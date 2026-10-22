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
    public void ChangeMasterVolume()
    {
        if (MasterVolumeSlider != null && MainAudioMixer != null)
            MainAudioMixer.SetFloat("MasterVolume", MasterVolumeSlider.value);
    }

    public void ChangeMusicVolume()
    {
        if (MusicVolumeSlider != null && MainAudioMixer != null)
            MainAudioMixer.SetFloat("MusicVolume", MusicVolumeSlider.value);
    }

    public void ChangeSFXVolume()
    {
        if (SFXVolumeSlider != null && MainAudioMixer != null)
            MainAudioMixer.SetFloat("SFXVolume", SFXVolumeSlider.value);
    }

    public void ExitSettingsMenu()
    {
        RevertUnsavedChanges();

        gameObject.SetActive(false);
    }

    public void SaveSettings()
    {
        //Volume
        if (MasterVolumeSlider != null) PlayerPrefs.SetFloat("MasterVolume", MasterVolumeSlider.value);
        if (MusicVolumeSlider != null) PlayerPrefs.SetFloat("MusicVolume", MusicVolumeSlider.value);
        if (SFXVolumeSlider != null) PlayerPrefs.SetFloat("SFXVolume", SFXVolumeSlider.value);

        //Text Speed + Size (through GameSettings so the dialogue updates too)
        GameSettings.Preview(TextSpeedIndex, TextSizeIndex);
        GameSettings.Save();

        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        //Volume
        if (MasterVolumeSlider != null && MainAudioMixer != null)
        {
            MasterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0f);
            MainAudioMixer.SetFloat("MasterVolume", MasterVolumeSlider.value);
        }

        if (MusicVolumeSlider != null && MainAudioMixer != null)
        {
            MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0f);
            MainAudioMixer.SetFloat("MusicVolume", MusicVolumeSlider.value);
        }

        if (SFXVolumeSlider != null && MainAudioMixer != null)
        {
            SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0f);
            MainAudioMixer.SetFloat("SFXVolume", SFXVolumeSlider.value);
        }

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
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);

        if (MasterVolumeSlider != null) MasterVolumeSlider.SetValueWithoutNotify(masterVolume);
        if (MusicVolumeSlider != null) MusicVolumeSlider.SetValueWithoutNotify(musicVolume);
        if (SFXVolumeSlider != null) SFXVolumeSlider.SetValueWithoutNotify(sfxVolume);

        if (MainAudioMixer != null)
        {
            MainAudioMixer.SetFloat("MasterVolume", masterVolume);
            MainAudioMixer.SetFloat("MusicVolume", musicVolume);
            MainAudioMixer.SetFloat("SFXVolume", sfxVolume);
        }

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
