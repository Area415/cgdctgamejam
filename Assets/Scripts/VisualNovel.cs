using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class VisualNovel : MonoBehaviour {
    [SerializeField] CharacterList characterList;
    [SerializeField] private Transform characterContainer;
    
    private Dictionary<string, Character> characters = new Dictionary<string, Character>();
    private DialogueRunner dialogueRunner; // utility object that serves lines of dialogue
    private FadeOverlay fadeOverlay; // black overlay used to fade in/out of scenes

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    //For the Audio Manager
    [SerializeField] private AudioManager audioManager;
    private bool hasWarnedMissingAudioManager = false;

    //For the Dialogue to Be Paused
    [SerializeField] private LineAdvancer lineAdvancer;
    private bool dialoguePaused = false;

    [SerializeField] private GameObject dialogueBoxUI;

    // when this visual novel object is created
    // (in our example, this happens when the scene is created)
    private void Awake() {
        // get handles of utility objects in the scene that we need
        dialogueRunner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        fadeOverlay = FindObjectOfType<FadeOverlay>();

        // Route the scene's music and SFX through the mixer so the volume sliders control them.
        if (musicSource != null && musicSource.outputAudioMixerGroup == null)
            musicSource.outputAudioMixerGroup = GameSettings.MusicGroup;
        if (sfxSource != null && sfxSource.outputAudioMixerGroup == null)
            sfxSource.outputAudioMixerGroup = GameSettings.SfxGroup;

        // <<camera NAME_OF_LOCATION>>
        // Uncomment this line to define the command:
        dialogueRunner.AddCommandHandler<Location>("camera", ChangeCameraLocation);
        
        // <<place NAME_OF_CHARACTER>>
        // Uncomment this line to define the command:
        dialogueRunner.AddCommandHandler<string>("place", PlaceCharacter);

        //This is for the character to exit:
        dialogueRunner.AddCommandHandler<string>("exit", ExitCharacter);

        // <<fadeIn DURATION>>
        // Uncomment this line to define the command:
        dialogueRunner.AddCommandHandler<float>("fadeIn", FadeIn);

        // <<fadeOut DURATION>>
        // Uncomment this line to define the command:
        dialogueRunner.AddCommandHandler<float>("fadeOut", FadeOut);

        // <<playMusic MUSIC_NAME>>
        dialogueRunner.AddCommandHandler<string>("playMusic", PlayMusic);

        // <<playSFX SFX_NAME>>
        dialogueRunner.AddCommandHandler<string>("playSFX", PlaySFX);

        // <<bike CHARACTER_NAME on|off>>
        dialogueRunner.AddCommandHandler<string, string>("bike", SetBikeVisibility);

        // <<stopMusic>>
        dialogueRunner.AddCommandHandler("stopMusic", StopMusic);

        // Cyberdeck HOME tab job card:
        // <<set_job "CURRENT JOB" "IN PROGRESS" "#4153">>
        dialogueRunner.AddCommandHandler<string, string, string>("set_job", CyberdeckHomeStatus.SetJob);
        // <<set_job_route "Boo Boo's Boba Emporium · Understory" "Penthouse · Meridian District">>
        dialogueRunner.AddCommandHandler<string, string>("set_job_route", CyberdeckHomeStatus.SetJobRoute);
        // <<set_job_payout "₿ 12.50 (−20% late fee)">>
        dialogueRunner.AddCommandHandler<string>("set_job_payout", CyberdeckHomeStatus.SetJobPayout);
        // <<set_job_progress 0.35>>   (0 = picked up, 1 = delivered)
        dialogueRunner.AddCommandHandler<float>("set_job_progress", CyberdeckHomeStatus.SetJobProgress);
        // <<set_rating 5 "1 new review">>   (0-5 stars)
        dialogueRunner.AddCommandHandler<float, string>("set_rating", CyberdeckHomeStatus.SetRating);

        //Loading other scenes
        dialogueRunner.AddCommandHandler<string>("loadScene", LoadScene);

        //Hide Dialogue at the end
        dialogueRunner.AddCommandHandler("hideDialogue", HideDialogue);

        //Show Dialogue box - will need this for the cyberdeck later
        dialogueRunner.AddCommandHandler("showDialogue", ShowDialogue);

        ResolveAudioManager();
    }

    private void ResolveAudioManager()
    {
        if (audioManager != null)
        {
            return;
        }

        audioManager = AudioManager.Instance;

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        if (audioManager == null && !hasWarnedMissingAudioManager)
        {
            Debug.LogWarning("No active AudioManager found for VisualNovel audio commands.");
            hasWarnedMissingAudioManager = true;
        }
    }

    // moves camera to camera location {location} in the scene
    private void ChangeCameraLocation(Location location) {
        Camera.main.transform.position = location.cameraMarker.position;
        Camera.main.transform.rotation = location.cameraMarker.rotation;

          foreach (var otherLocation in FindObjectsOfType<Location>()) {
        if (otherLocation.background != null) {
            otherLocation.background.SetActive(otherLocation == location);
        }
      }

        // Update the cyberdeck Home tab's location (skipped if District Name is blank).
        CyberdeckHomeStatus.SetLocation(location);
    }

    // looks for character named {characterName} and moves it to the location 
    // of marker named {markerName} in the scene
    private void PlaceCharacter(string characterName) {
        characterName = characterName.Trim();
        Character character;

        // if this character has not been instantiated before, do so now
        if (!characters.ContainsKey(characterName)) {
            var characterPrefab = characterList.FindCharacterPrefab(characterName);
            character = Instantiate(characterPrefab, characterContainer); 
            
            // and place it in the list of characters so we can find it next time
            characters[characterName] = character;
            
            character.name = characterName;
        } else {
            // otherwise get the one we prepared earlier
            character = characters[characterName];
        }
    }

    private void ExitCharacter(string characterName) {
        characterName = characterName.Trim();

        if (characters.ContainsKey(characterName)) {
            Destroy(characters[characterName].gameObject);
            characters.Remove(characterName);
        }
    }

    // fades in a black screen over {time} seconds
    private Coroutine FadeIn(float time = 1f) {
        FindAnyObjectByType<SkipModeController>()?.ForceOff();
        return StartCoroutine(fadeOverlay.FadeIn(time));
    }

    // fades out a black screen over {time} seconds
    private Coroutine FadeOut(float time = 1f) {
        return StartCoroutine(fadeOverlay.FadeOut(time));
    }

    // plays background music {musicName}
    private void PlayMusic(string musicName) {
        musicName = musicName.Trim(); 

        ResolveAudioManager();
        if (audioManager == null || musicSource == null)
        {
            return;
        }
        
        AudioClip clip = audioManager.GetMusic(musicName);
        
        if (clip != null) {
            musicSource.clip = clip;
            musicSource.loop = true; 
            musicSource.Play();
        }
    }

    // plays sound effect {sfxName}
    private void PlaySFX(string sfxName) {
        sfxName = sfxName.Trim();

        ResolveAudioManager();
        if (audioManager == null || sfxSource == null)
        {
            return;
        }
        
        AudioClip clip = audioManager.GetSFX(sfxName);
        
        if (clip != null) {
            sfxSource.PlayOneShot(clip); 
        }
    }

    private void SetBikeVisibility(string characterName, string visibility) {
        characterName = characterName.Trim();
        visibility = visibility.Trim();

        if (!characters.TryGetValue(characterName, out Character character)) {
            Debug.LogWarning($"Cannot toggle bike; character '{characterName}' is not currently placed.");
            return;
        }

        bool showBike = visibility.Equals("on", StringComparison.InvariantCultureIgnoreCase)
            || visibility.Equals("show", StringComparison.InvariantCultureIgnoreCase)
            || visibility.Equals("true", StringComparison.InvariantCultureIgnoreCase);

        character.SetBikeVisible(showBike);
    }

    // stops the current background music
    private void StopMusic() {
        musicSource.Stop();
        musicSource.clip = null;
    }

    //Loads the scene we want next
    private void LoadScene(string sceneName) {
        sceneName = sceneName.Trim();
        SceneManager.LoadScene(sceneName);
    }

    private void HideDialogue() {
        if (dialogueBoxUI != null) {
            dialogueBoxUI.SetActive(false);
        }
    }

    private void ShowDialogue() {
        if (dialogueBoxUI != null) {
            dialogueBoxUI.SetActive(true);
        }
    }

    public void PauseDialogue(bool isPaused)
    {
        dialoguePaused = isPaused;
        
        if (lineAdvancer != null)
        {
            lineAdvancer.enabled = !dialoguePaused;
        }
    }

    public bool IsDialoguePaused()
    {
        return dialoguePaused;
    }
}