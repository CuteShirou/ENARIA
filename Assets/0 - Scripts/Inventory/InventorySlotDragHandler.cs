using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotView))]
public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventorySlotView slotView;
    private Canvas parentCanvas;
    private RectTransform dragIcon;

    private void Awake()
    {
        slotView = GetComponent<InventorySlotView>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotView.Get() == null) return; // rien à drag

        // crée une icône temporaire à suivre
        dragIcon = new GameObject("DragIcon", typeof(RectTransform)).GetComponent<RectTransform>();
        dragIcon.SetParent(parentCanvas.transform, false);
        var image = dragIcon.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = slotView.Get().icon;
        image.raycastTarget = false;
        dragIcon.sizeDelta = new Vector2(64, 64);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var targetSlot = GetComponent<InventorySlotView>();
        var sourceHandler = eventData.pointerDrag?.GetComponent<InventorySlotDragHandler>();

        if (sourceHandler != null && targetSlot != null && sourceHandler.slotView != targetSlot)
        {
            InventoryManager.Instance.SwapItems(sourceHandler.slotView.Index, targetSlot.Index);
        }
    }
}