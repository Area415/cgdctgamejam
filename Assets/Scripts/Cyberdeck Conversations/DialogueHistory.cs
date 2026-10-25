using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// The cyberdeck HISTORY panel.
///
/// Lines are recorded in ONE shared list for the whole play session, no matter
/// which scene is running (TristynScene has no History panel, but its lines are
/// still recorded). Each scene's History panel shows that shared list, so the
/// history carries over when the game loads a new scene.
///
/// Starting from the main menu clears it (MainMenuManager calls ClearAll()).
/// </summary>
public class DialogueHistory : MonoBehaviour 
{
    [SerializeField] private Transform historyContentContainer;
    [SerializeField] private GameObject historyLinePrefab;
    [SerializeField] private ScrollRect historyScrollRect;

    // ---------- Shared, session-wide record ----------

    private const int MaxLines = 500; // keep the panel light
    private static readonly List<string> Lines = new List<string>();
    private static event System.Action<string> LineAdded;
    private static int totalRecorded; // counts every line ever recorded this session

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Lines.Clear();
        totalRecorded = 0;
        LineAdded = null;
        // Listen for every delivered line, in every scene, for the whole session.
        Yarn.Unity.LinePresenter.OnLineDelivered -= Record;
        Yarn.Unity.LinePresenter.OnLineDelivered += Record;
    }

    /// <summary>Forget everything (called when the main menu loads).</summary>
    public static void ClearAll()
    {
        Lines.Clear();
        totalRecorded = 0;
    }

    private static void Record(string characterName, string text)
    {
        string lineText = text;
        
        if (!string.IsNullOrEmpty(characterName))
        {
            string prefix = characterName + ":";
            if (lineText.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                lineText = lineText.Substring(prefix.Length).TrimStart();
            }

            string hexColor = GetColorForCharacter(characterName);
            lineText = $"<b><color={hexColor}>{characterName.ToUpper()}</color></b>\n{lineText}";    
        }

        Lines.Add(lineText);
        totalRecorded++;
        if (Lines.Count > MaxLines) Lines.RemoveAt(0);
        LineAdded?.Invoke(lineText);
    }

    // ---------- This scene's panel ----------

    private int shownTotal; // how many of the recorded lines this panel already shows

    private void OnEnable()
    {
        CatchUp();
        LineAdded += OnLineAdded;
    }

    private void OnDisable()
    {
        LineAdded -= OnLineAdded;
    }

    private void OnLineAdded(string line)
    {
        AddLineToHistory(line);
        shownTotal = totalRecorded;
    }

    // Show any lines recorded while this panel was hidden or before this scene loaded.
    private void CatchUp()
    {
        int firstInList = totalRecorded - Lines.Count;
        for (int t = Mathf.Max(shownTotal, firstInList); t < totalRecorded; t++)
            AddLineToHistory(Lines[t - firstInList]);
        shownTotal = totalRecorded;
    }

    private void AddLineToHistory(string formattedText)
    {
        if (historyContentContainer == null || historyLinePrefab == null) return;

        GameObject newLine = Instantiate(historyLinePrefab, historyContentContainer);
        TMP_Text tmp = newLine.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = formattedText;
            SettingsTextScaler.AddTo(tmp); // follow the player's Text Size setting
        }

        if (historyScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            historyScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private static string GetColorForCharacter(string charName)
    {
        string nameLower = charName.Trim().ToLower();

        switch (nameLower)
        {
            case "incompetent director":
                return "#6AB13A";
            case "val":
                return "#BB3DBC";
            case "nat":
                return "#7F35C7";
            case "t.a.b":
                return "#3DADFF";
            case "nubs":
                return "#3DADFF";
            case "queenie":
                return "#E2629B";
            case "maxx":
                return "#00FFF6";
            case "blank":
                return "#989898";
            case "benji":
                return "#015693";
            case "kaya":
                return "#FBB322";
            case "luna":
                return "#FF0000";
            case "cybergull":
                return "#EFF235";
            case "the big bad":
                return "#6379C1";
            default:
                return "#FFFFFF";
        }
    }
}
