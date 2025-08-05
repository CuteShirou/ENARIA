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

        iconImage = transform.Find("Icon").GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
        }

        tooltip = FindFirstObjectByType<SkillTooltipUI>();

        UpdateVisual();

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void UpdateVisual()
    {
        Button btn = GetComponent<Button>();
        btn.interactable = manager.CanUnlock(node);
        iconImage.color = node.isUnlocked ? Color.white : Color.gray;
    }

    private void OnClick()
    {
        manager.UnlockSkill(node);
        manager.UpdateAllSkillButtons();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
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
