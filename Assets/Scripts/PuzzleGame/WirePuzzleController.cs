using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

public class WirePuzzleController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public GameObject puzzleCanvas;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text barkText;
    [SerializeField] private TMP_Text lampText;
    [SerializeField] private Image panelBackgroundRenderer;
    [SerializeField] private Image statusContainerImage;
    [SerializeField] private Image lampSpriteImage;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject recallButton;
    [SerializeField] private Transform wireContainer;

    [Header("Panel State Sprites")]
    [SerializeField] private Sprite panelDefault;
    [SerializeField] private Sprite panelError;
    [SerializeField] private Sprite panelFail;
    [SerializeField] private Sprite panelWin;

    [Header("Status Bar Sprites")]
    [SerializeField] private Sprite statusDefaultSprite;
    [SerializeField] private Sprite statusErrorSprite;
    [SerializeField] private Sprite statusFailSprite;
    [SerializeField] private Sprite statusWinSprite;

    [Header("Lamp Sprites")]
    [SerializeField] private Sprite redLampSprite;
    [SerializeField] private Sprite greenLampSprite;

    [Header("Wire Sprites")]
    [SerializeField] private Sprite redLitSprite;

    [Header("Audio / SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip gullCrySSfx; 
    [SerializeField] private AudioClip gullCryMSfx; 
    [SerializeField] private AudioClip gullFlapSfx;
    [SerializeField] private AudioClip lockdownSfx;
    [SerializeField] private AudioClip accessGrantedSfx;
    [SerializeField] private AudioClip wireTouchSfx;

    [Header("Yarn Things")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private VisualNovel visualNovel;

    private readonly List<string> correctSequence = new List<string> { "Red", "Blue", "Yellow", "Green" };
    private int currentStep = 0;
    private int failCount = 0;
    private bool isPuzzleActive = false;
    private bool isProcessingError = false;
    private bool isFinished = false;

    private Color defaultTextColor;

    private Dictionary<Transform, Sprite> originalLitSprites = new Dictionary<Transform, Sprite>();

    private void Awake()
    {
        if (visualNovel == null)
        {
            visualNovel = FindObjectOfType<VisualNovel>();
        }
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        }

        if (statusText != null)
        {
            defaultTextColor = statusText.color;
        }

        if (puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(false);
        }

        if (wireContainer != null)
        {
            foreach (Transform wireObj in wireContainer)
            {
                Transform litChild = wireObj.Find("Lit");
                if (litChild != null)
                {
                    var img = litChild.GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        originalLitSprites[wireObj] = img.sprite;
                    }
                }
            }
        }
    }

    [YarnCommand("wait_for_wire_puzzle")]
    public static IEnumerator RegisterPuzzleCommand()
    {
        var controller = FindObjectOfType<WirePuzzleController>(true);
        if (controller != null)
        {
            controller.StartPuzzle();

            while (controller.isPuzzleActive)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("WirePuzzleController not found!");
        }
    }

    public void StartPuzzle()
    {
        currentStep = 0;
        failCount = 0;
        isPuzzleActive = true;
        isProcessingError = false;
        isFinished = false;

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        if (recallButton != null)
        {
            recallButton.SetActive(true);
        }

        SetState("SYSTEM: LOCKED - 3 ATTEMPTS REMAINING", panelDefault, statusDefaultSprite, defaultTextColor);
        SetLampState("LOCKED", redLampSprite);
        SetBark("It’s definitely some sort of sequence. Better be careful where I touch...");
        ResetAllWires();
        
        if (visualNovel != null)
        {
            visualNovel.PauseDialogue(true);
        }
        SetDialogueInputActive(false);

        if (puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(true);
        }
    }

    public void OnWireClicked(string wireColor)
    {
        if (dialogueRunner == null || isProcessingError || isFinished) return;

                PlaySFX(wireTouchSfx);

        if (correctSequence[currentStep] == wireColor)
        {
            SetWireLit(wireColor, true);

            if (currentStep == 0) SetBark("That seems right. Now what’s next?");
            else if (currentStep == 1) SetBark("Good, I’m getting closer.");
            else if (currentStep == 2) SetBark("Yes! Come on...");
            else if (currentStep == 3) SetBark("Hell yeah! This flux ring is mine.");

            currentStep++;

            if (currentStep >= correctSequence.Count)
            {
                SetLampState("UNLOCKED", greenLampSprite);
                PlaySFX(accessGrantedSfx);
                TriggerFinishState(true, true, "$puzzleSolved", true, "$hasFluxRing", true, "$gullOwesLuna", true, "ACCESS GRANTED", panelWin, statusWinSprite, Color.green);
            }
        }
        else
        {
            failCount++;
            dialogueRunner.VariableStorage.SetValue("$wire_fails", failCount);

            if (failCount == 1)
            {
                SetBark("Shit, that wasn’t right. I think Val mentioned something about expensive parts? Maybe I should try and recall what she said...");
                PlaySFX(gullCrySSfx);
                PlaySFX(gullFlapSfx);
                StartCoroutine(HandleErrorSequence("ERROR: TWO MORE ATTEMPTS", "SYSTEM LOCKED: 2 ATTEMPTS REMAINING", panelError, statusErrorSprite, "Let's try again—and not piss off the cybergull."));
            }
            else if (failCount == 2)
            {
                SetBark("The cybergull is freaking out! I need to lock in.");
                PlaySFX(gullCryMSfx);
                PlaySFX(gullFlapSfx);
                StartCoroutine(HandleErrorSequence("ERROR:ONE MORE ATTEMPT", "SYSTEM LOCKED: 1 ATTEMPT REMAINING", panelError, statusErrorSprite, "One more chance to crack this damn thing."));
            }
            else if (failCount >= 3)
            {
                SetBark("Damn, the system locked me out. I better not touch it anymore.");
                SetLampState("LOCKED DOWN", redLampSprite);
                PlaySFX(lockdownSfx);
                TriggerFinishState(false, false, "$puzzleSolved", false, "$gullOwesLuna", false, null, false, "SEQUENCE FAILED: SYSTEM LOCKDOWN", panelFail, statusFailSprite, Color.red);
            }
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    private void SetBark(string message)
    {
        if (barkText != null)
        {
            barkText.text = message;
        }
    }

    private void SetLampState(string labelText, Sprite lampSprite)
    {
        if (lampText != null)
        {
            lampText.text = labelText;
        }
        if (lampSpriteImage != null && lampSprite != null)
        {
            lampSpriteImage.sprite = lampSprite;
        }
    }

    private void TriggerFinishState(bool puzzleSolvedVal, bool hasFluxVal, string var1Name, bool var1Val, string var2Name, bool var2Val, string var3Name, bool var3Val, string statusMsg, Sprite panelSprite, Sprite statusSprite, Color textColor)
    {
        isFinished = true;
        
        if (!string.IsNullOrEmpty(var1Name)) dialogueRunner.VariableStorage.SetValue(var1Name, var1Val);
        if (!string.IsNullOrEmpty(var2Name)) dialogueRunner.VariableStorage.SetValue(var2Name, var2Val);
        if (!string.IsNullOrEmpty(var3Name)) dialogueRunner.VariableStorage.SetValue(var3Name, var3Val);

        SetState(statusMsg, panelSprite, statusSprite, textColor);

        if (recallButton != null)
        {
            recallButton.SetActive(false);
        }
        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }
    }

    public void OnContinueButtonClicked()
    {
        if (!isFinished) return;

        if (puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        if (recallButton != null)
        {
            recallButton.SetActive(true);
        }

        var fadeGroups = FindObjectsOfType<CanvasGroup>();
        foreach (var group in fadeGroups)
        {
            if (group.gameObject.name.Contains("Fade") || group.gameObject.name.Contains("Black"))
            {
                group.alpha = 0f;
            }
        }

        if (visualNovel != null)
        {
            visualNovel.PauseDialogue(false);
        }
        SetDialogueInputActive(true);

        isPuzzleActive = false;
    }

    private void SetWireLit(string colorName, bool isLit)
    {
        if (wireContainer == null) return;

        string targetObjectName = "Wire_" + colorName.ToLower();
        Transform wireObj = wireContainer.Find(targetObjectName);

        if (wireObj == null)
        {
            foreach (Transform child in wireContainer)
            {
                if (child.name.Equals(targetObjectName, System.StringComparison.OrdinalIgnoreCase))
                {
                    wireObj = child;
                    break;
                }
            }
        }

        if (wireObj != null)
        {
            Transform litChild = wireObj.Find("Lit");
            if (litChild != null)
            {
                litChild.gameObject.SetActive(isLit);
                var img = litChild.GetComponent<Image>();
                if (img != null && originalLitSprites.ContainsKey(wireObj))
                {
                    img.sprite = originalLitSprites[wireObj];
                }
            }
        }
    }

    private void FlashAllWiresRed()
    {
        if (wireContainer == null) return;

        foreach (Transform wireObj in wireContainer)
        {
            Transform litChild = wireObj.Find("Lit");
            if (litChild != null)
            {
                litChild.gameObject.SetActive(true); 
                var img = litChild.GetComponent<Image>();
                if (img != null && redLitSprite != null)
                {
                    img.sprite = redLitSprite;
                }
            }
        }
    }

    private void ResetAllWires()
    {
        if (wireContainer == null) return;

        foreach (Transform wireObj in wireContainer)
        {
            Transform litChild = wireObj.Find("Lit");
            if (litChild != null)
            {
                litChild.gameObject.SetActive(false);
                var img = litChild.GetComponent<Image>();
                if (img != null && originalLitSprites.ContainsKey(wireObj))
                {
                    img.sprite = originalLitSprites[wireObj];
                }
            }
        }
    }

    private IEnumerator HandleErrorSequence(string errorMsg, string nextCountdownMsg, Sprite errorSprite, Sprite statusErrorSp, string resetBarkMsg)
    {
        isProcessingError = true;
        
        FlashAllWiresRed();
        SetState(errorMsg, errorSprite, statusErrorSp, Color.red);
        
        yield return new WaitForSeconds(2.5f);

        currentStep = 0;
        ResetAllWires();

        SetState(nextCountdownMsg, panelDefault, statusDefaultSprite, defaultTextColor);
        SetBark(resetBarkMsg);
        isProcessingError = false;
    }

    private void SetState(string message, Sprite targetPanelSprite, Sprite targetStatusSprite, Color textColor)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = textColor;
        }
        if (panelBackgroundRenderer != null && targetPanelSprite != null)
        {
            panelBackgroundRenderer.sprite = targetPanelSprite;
        }
        if (statusContainerImage != null && targetStatusSprite != null)
        {
            statusContainerImage.sprite = targetStatusSprite;
        }
    }

    private void SetDialogueInputActive(bool active)
    {
        var advancers = FindObjectsOfType<ClickToAdvance>();
        foreach (var advancer in advancers)
        {
            advancer.enabled = active;
        }
    }

    public void OnRecallButtonClicked()
    {
        // Find the cyberdeck that's actually in this scene (it may be inactive, so
        // FindObjectOfType won't see it). FindObjectsOfTypeAll also returns prefab
        // assets, so skip anything that isn't part of a loaded scene.
        CyberdeckController cyberdeck = null;
        foreach (var c in Resources.FindObjectsOfTypeAll<CyberdeckController>())
        {
            if (c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded)
            {
                cyberdeck = c;
                break;
            }
        }
        
        if (cyberdeck != null)
        {
            cyberdeck.OpenCyberdeckGeneral();
            cyberdeck.ToggleHistoryOverlay();
        }
        else
        {
            Debug.LogError("CyberdeckController not found in the scene!");
        }
    }
}
