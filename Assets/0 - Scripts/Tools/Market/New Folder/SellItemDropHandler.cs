using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SellItemDropHandler : MonoBehaviour, IDropHandler
{
    [Tooltip("Lien vers le panel UI qui affiche l'item droppé (SellItemPanel)")]
    public SellItemPanel sellItemPanel;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // On récupère le InventorySlotView qui était draggué (c'est le slot preview)
        var slotView = eventData.pointerDrag.GetComponent<InventorySlotView>();
        if (slotView == null)
        {
            // parfois le drag est sur un enfant -> essayer parent
            slotView = eventData.pointerDrag.GetComponentInParent<InventorySlotView>();
            if (slotView == null) return;
        }

        int sourceIndex = slotView.Index;
        var item = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemAt(sourceIndex) : null;
        var count = InventoryManager.Instance != null ? InventoryManager.Instance.GetCountAt(sourceIndex) : 0;

        if (item == null || count <= 0)
        {
            Debug.LogWarning("[SellItemDropHandler] Item invalide ou quantité nulle.");
            return;
        }

        if (sellItemPanel != null)
            sellItemPanel.SetPendingItem(item, sourceIndex, count);
    }
}
