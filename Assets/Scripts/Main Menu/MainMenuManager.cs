using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public Canvas SettingsMenu;
    public Canvas CreditsMenu;
    public Canvas LoadGameMenu;

    [Header("Save System")]
    public Button continueButton;

    void Start()
    {
         // Back at the title screen: start the next run with an empty history.
         DialogueHistory.ClearAll();

         if (SettingsMenu != null) {
                SettingsMenu.gameObject.SetActive(false);
         }
         if (CreditsMenu != null) {
                CreditsMenu.gameObject.SetActive(false);
         }
         if (LoadGameMenu != null) {
                LoadGameMenu.gameObject.SetActive(false);
         }

         SaveManager saveManager = FindObjectOfType<SaveManager>();
         if (continueButton != null && saveManager != null)
         {
             continueButton.interactable = saveManager.HasSaveData();
         }
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("LoadState", 0);
        SceneManager.LoadScene("IntroScene");
    }

    public void ContinueGame()
    {
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null && saveManager.HasSaveData())
        {
            saveManager.LoadGame();
        }
    }

    public void EnterSettingsMenu()
    {
         if (SettingsMenu != null){
                SettingsMenu.gameObject.SetActive(true);
         }
    }

    public void EnterCreditsMenu()
    {
         if (CreditsMenu != null){
                CreditsMenu.gameObject.SetActive(true);
         }
    }

    public void EnterLoadGameMenu()
    {
         if (LoadGameMenu != null){
                LoadGameMenu.gameObject.SetActive(true);
         }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("TristynScene");
    }
}
