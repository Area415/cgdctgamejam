using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tints every visible graphic (images + text) under this object while the
/// mouse hovers over it, then puts the original colors back.
///
/// Use it when a button is made of several pieces (e.g. "CYBERDECK" + "3000")
/// and Unity's Button Color Tint can only tint one of them.
/// Put it on the same object as the Button.
///
/// It only highlights when the Button actually does something: the Button must
/// be interactable AND have at least one On Click entry. So in a scene where the
/// button isn't hooked up yet (e.g. the restricted intro in TristynScene),
/// there's no hover, and it doesn't look clickable.
/// </summary>
public class HoverTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color hoverColor = new Color32(0x3F, 0xCF, 0xC1, 0xFF); // #3FCFC1

    private readonly List<Graphic> graphics = new List<Graphic>();
    private readonly List<Color> originalColors = new List<Color>();
    private bool hovering;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        foreach (var g in GetComponentsInChildren<Graphic>(true))
        {
            if (g.gameObject == gameObject) continue; // skip the invisible click area itself
            graphics.Add(g);
            originalColors.Add(g.color);
        }
    }

    // Clickable = interactable AND at least one On Click entry with a real target
    // (an entry whose object slot says "None" doesn't count).
    private bool IsClickable
    {
        get
        {
            if (button == null) return true;
            if (!button.IsInteractable()) return false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentTarget(i) != null) return true;
            return false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsClickable) return;
        hovering = true;
        for (int i = 0; i < graphics.Count; i++)
        {
            // keep each piece's own transparency, only change the color
            var c = hoverColor;
            c.a = originalColors[i].a;
            graphics[i].color = c;
        }
    }

    public void OnPointerExit(PointerEventData eventData) => Restore();

    private void OnDisable() => Restore(); // e.g. the cyberdeck opens while hovering

    private void Restore()
    {
        if (!hovering) return;
        hovering = false;
        for (int i = 0; i < graphics.Count; i++)
            if (graphics[i] != null) graphics[i].color = originalColors[i];
    }
}
