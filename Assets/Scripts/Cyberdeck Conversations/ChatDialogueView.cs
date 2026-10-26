using System.Threading;
using UnityEngine;
using System;
using Yarn.Unity;

#nullable enable

public class ChatDialogueView : DialoguePresenterBase
{
    [Header("Prefabs")]
    [SerializeField] SerializableDictionary<string, ChatDialogueViewBubble?> characters = new();

    [Space, SerializeField] ChatDialogueViewBubble? defaultBubblePrefab = null;

    [SerializeField] ChatDialogueViewOptionsButton? optionsButtonPrefab;

    [Header("Containers")]
    [SerializeField] RectTransform? bubbleContainer;

    [SerializeField] RectTransform? optionsContainer;

    [Header("Timing")]
    [SerializeField] float delayAfterLine = 1f;

    [SerializeField] float minimumTypingDelay = 0.5f;

    [SerializeField] float maximumTypingDelay = 3f;

    [SerializeField] float typingDelayPerCharacter = 0.05f;

    [SerializeField] bool showTypingIndicators = true;

    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        if (bubbleContainer == null)
        {
            Debug.LogWarning($"Can't show line '{line.Text.Text}': no bubble container");
            return;
        }

        var prefab = defaultBubblePrefab;

        if (line.CharacterName != null && characters.TryGetValue(line.CharacterName, out var characterBubble))
        {
            prefab = characterBubble;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"Can't show line '{line.Text.Text}': no default bubble was set");
            return;
        }

        if (showTypingIndicators && prefab.HasIndicator)
        {
            var typingBubble = Instantiate(prefab, bubbleContainer);
            typingBubble.ShowTyping();

            var typingDelay = Mathf.Clamp(
                line.TextWithoutCharacterName.Text.Length * typingDelayPerCharacter,
                minimumTypingDelay,
                maximumTypingDelay);

            // Faster Text Speed setting = shorter "typing..." wait.
            typingDelay /= GameSettings.SpeedMultiplier;

            await YarnTask.Delay(TimeSpan.FromSeconds(typingDelay), token.HurryUpToken).SuppressCancellationThrow();

            Destroy(typingBubble.gameObject);
        }

        var bubble = Instantiate(prefab, bubbleContainer);
        bubble.ShowText(line.TextWithoutCharacterName.Text);

        /* For Alyssa to review if keep or not!
        if (AudioManager.Instance != null)
        {
            AudioClip bubbleSfx = AudioManager.Instance.GetSFX("bubblePop"); 
            if (bubbleSfx != null)
            {
                GameObject tempAudio = new GameObject("TempBubbleAudio");
                AudioSource source = tempAudio.AddComponent<AudioSource>();
                source.spatialBlend = 0f;
                source.PlayOneShot(bubbleSfx);
                Destroy(tempAudio, bubbleSfx.length);
            }
        }*/

        Canvas.ForceUpdateCanvases();
        var scrollRect = bubbleContainer.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        await YarnTask.WaitUntilCanceled(token.NextContentToken).SuppressCancellationThrow();
    }

    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken cancellationToken)
    {
        if (optionsContainer == null)
        {
            Debug.LogWarning($"Can't show options: no bubble container");
            return null;
        }

        if (optionsButtonPrefab == null)
        {
            Debug.LogWarning($"Can't show options: no bubble prefab");
            return null;
        }

        for (int i = 0; i < optionsContainer.childCount; i++)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }

        var completionSource = new YarnTaskCompletionSource<DialogueOption>();

        foreach (var option in dialogueOptions)
        {
            var button = Instantiate(optionsButtonPrefab, optionsContainer);
            button.Text = option.Line.TextWithoutCharacterName.Text;

            button.OnClick = () => completionSource.TrySetResult(option);
        }

        var selectedOption = await completionSource.Task;

        for (int i = 0; i < optionsContainer.childCount; i++)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }

        return selectedOption;
    }

    public void ClearChatHistory()
    {
        if (bubbleContainer != null)
        {
            foreach (Transform child in bubbleContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    //For the phone batches
    public void SpawnInstantMessage(string characterName, string messageText)
    {
        if (bubbleContainer == null) return;

        var prefab = defaultBubblePrefab;
        if (characterName != null && characters.TryGetValue(characterName, out var characterBubble))
        {
            prefab = characterBubble;
        }

        if (prefab == null) return;

        var bubble = Instantiate(prefab, bubbleContainer);
        bubble.ShowText(messageText);

        Canvas.ForceUpdateCanvases();
        var scrollRect = bubbleContainer.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
