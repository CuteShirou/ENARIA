using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentAfterDrag;
    public InventorySlot parentSlot;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = GetComponentInParent<InventorySlot>();
        parentAfterDrag = transform.parent; // ← garde ItemIcon comme référence
        transform.SetParent(canvas.transform, false);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        var dropTarget = eventData.pointerEnter?.GetComponentInParent<InventorySlot>();
        if (dropTarget != null)
        {
            parentAfterDrag = dropTarget.transform;
        }

        transform.SetParent(parentAfterDrag, false);
        rectTransform.anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
    }

}