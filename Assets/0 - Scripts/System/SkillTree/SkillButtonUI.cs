using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SkillNode node;
    private SkillTreeManager manager;
    private Image iconImage;

    private SkillTooltipUI tooltip;

    public void Initialize(SkillNode node, SkillTreeManager manager)
    {
        this.node = node;
        this.manager = manager;

        // Cherche l'image "Icon" dans la hiérarchie (protégé)
        Transform iconT = transform.Find("Icon");
        if (iconT != null)
            iconImage = iconT.GetComponent<Image>();
        else
            iconImage = GetComponentInChildren<Image>(); // fallback

        if (iconImage != null)
        {
            iconImage.sprite = node?.Icon;
        }

        // FindFirstObjectByType existe sur les versions récentes d'Unity (2023+). Si tu as une version plus vieille,
        // remplace par FindObjectOfType<SkillTooltipUI>().
        tooltip = FindObjectOfType<SkillTooltipUI>();

        UpdateVisual();

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }

    public void UpdateVisual()
    {
        if (node == null || manager == null)
            return;

        Button btn = GetComponent<Button>();

        bool unlocked = node.IsUnlocked;           // UTILISER la propriété IsUnlocked
        bool canUnlock = manager.CanUnlock(node);  // conditions pour rendre le bouton cliquable

        if (btn != null)
        {
            // si déjà déverrouillé -> non interactif
            btn.interactable = !unlocked && canUnlock;
        }

        if (iconImage != null)
        {
            // couleur blanche si déverrouillé, gris sinon
            iconImage.color = unlocked ? Color.white : Color.gray;
        }

        // TODO: afficher un visuel supplémentaire pour "déverrouillé" (check, overlay, etc.)
    }

    private void OnClick()
    {
        if (node == null || manager == null) return;

        // Protection: n'essaye d'unlock que si possible
        if (manager.CanUnlock(node))
        {
            manager.UnlockSkill(node);
            manager.UpdateAllSkillButtons();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && node != null)
        {
            Vector2 screenPos = Input.mousePosition;
            tooltip.Show(node.SkillName, node.Description, node.Specifications, screenPos);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }
}








//using System.Collections.Generic;
//using Mirror;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    private SkillNode node;
//    private SkillTreeManager manager;
//    private int index;
//    private Image iconImage;
//    private SkillTooltipUI tooltip;
//    private bool unlocked;

//    public void Initialize(SkillNode node, SkillTreeManager manager, int index, bool isUnlocked)
//    {
//        this.node = node;
//        this.manager = manager;
//        this.index = index;
//        unlocked = isUnlocked;

//        iconImage = transform.Find("Icon").GetComponent<Image>();
//        if (iconImage) iconImage.sprite = node.Icon;

//        tooltip = FindFirstObjectByType<SkillTooltipUI>();

//        var btn = GetComponent<Button>();
//        btn.onClick.RemoveAllListeners();
//        btn.onClick.AddListener(OnClick);

//        UpdateVisual(manager.AvailablePoints, unlocked);
//    }

//    public void SetUnlocked(bool isUnlocked)
//    {
//        unlocked = isUnlocked;
//        node.isUnlocked = isUnlocked;
//        UpdateVisual(manager.AvailablePoints, unlocked);
//    }

//    public void UpdateVisual(int points, bool isUnlocked)
//    {
//        var btn = GetComponent<Button>();
//        btn.interactable = !isUnlocked && points >= node.cost;
//        if (iconImage) iconImage.color = isUnlocked ? Color.white : Color.gray;
//    }

//    private void OnClick()
//    {
//        manager.RequestUnlock(index);
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        tooltip?.Show(node.SkillName, node.Description, node.Specifications, Input.mousePosition);
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        tooltip?.Hide();
//    }
//}
