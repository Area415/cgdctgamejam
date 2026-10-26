using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

[System.Serializable]
public class ChatMessageEntry
{
    public string characterName;
    [TextArea] public string messageText;
}

[System.Serializable]
public class ChatBatch
{
    public string batchId;
    public List<ChatMessageEntry> messages = new List<ChatMessageEntry>();
}

public class PhoneChatManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueRunner? dialogueRunner;
    [SerializeField] private ChatDialogueView? chatView;

    [Header("Yarn Batches")]
    [SerializeField] private List<ChatBatch> allBatches = new List<ChatBatch>();

    //How the system can see which player unlocked what batch
    private HashSet<string> unlockedBatchIds = new HashSet<string>();

    private void Start()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler<string>("unlock_chat_batch", UnlockBatch);
        }
    }

    private void UnlockBatch(string batchId)
    {
        if (!unlockedBatchIds.Contains(batchId))
        {
            unlockedBatchIds.Add(batchId);
            Debug.Log($"Phone Chat: Unlocked new batch: {batchId}");
        }
    }

    //Should occur everytime the player opens up the cyberdeck (not basing off of notifcation button clicks)
    public void RefreshMessagesTab()
    {
        if (chatView == null) return;

        chatView.ClearChatHistory();

        foreach (var batch in allBatches)
        {
            if (unlockedBatchIds.Contains(batch.batchId))
            {
                foreach (var msg in batch.messages)
                {
                    chatView.SpawnInstantMessage(msg.characterName, msg.messageText);
                }
            }
        }
    }
}
