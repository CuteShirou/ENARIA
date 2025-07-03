using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventorySlot parentSlot;
    public Transform parentAfterDrag;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); // ← important : canvas local correct
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = GetComponentInParent<InventorySlot>();
        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ← déplacement correct prenant en compte le scale du canvas
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
    }
}