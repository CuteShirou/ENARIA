using UnityEngine;
using UnityEngine.EventSystems;

public class TimelineSlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetEntity;         // Entité représentée
    public InfoBubbleUI infoBubble;         // Référence à la bulle d'infos
    public Sprite portraitSprite;           // Portrait à afficher

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetEntity == null || infoBubble == null) return;

        CombatStats stats = targetEntity.GetComponent<CombatStats>();
        if (stats == null) return;

        infoBubble.gameObject.SetActive(true);
        infoBubble.SetInfo(stats, portraitSprite, targetEntity.name, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoBubble != null)
            infoBubble.gameObject.SetActive(false);
    }
}
