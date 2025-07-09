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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (parentSlot.currentItem != null && parentSlot.currentItem.quantity > 1)
            {
                parentSlot.currentItem.quantity -= 1;
                parentSlot.SetItem(parentSlot.currentItem);

                InventoryItem splitItem = new InventoryItem
                {
                    itemID = parentSlot.currentItem.itemID,
                    icon = parentSlot.currentItem.icon,
                    quantity = 1,
                    maxStack = parentSlot.currentItem.maxStack,
                    type = parentSlot.currentItem.type
                };

                GameObject clone = Instantiate(gameObject, canvas.transform);
                DraggableItem dragScript = clone.GetComponent<DraggableItem>();

                dragScript.parentSlot = parentSlot;
                dragScript.parentAfterDrag = parentSlot.transform;
                dragScript.canvasGroup.blocksRaycasts = false;

                clone.GetComponent<RectTransform>().position = Input.mousePosition;
                dragScript.OnBeginDrag(eventData);

                InventorySlot cloneSlot = clone.GetComponentInParent<InventorySlot>();
                if (cloneSlot != null) cloneSlot.SetItem(splitItem);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = GetComponentInParent<InventorySlot>();
        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform, false);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        InventorySlot targetSlot = eventData.pointerEnter?.GetComponentInParent<InventorySlot>();

        if (targetSlot != null)
        {
            parentAfterDrag = targetSlot.transform;
        }
        else
        {
            parentAfterDrag = parentSlot.transform;
        }

        transform.SetParent(parentAfterDrag, true);
        rectTransform.anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;

        // IMPORTANT : replace le visuel dans le bon slot
        if (parentSlot != null)
        {
            parentSlot.SetItem(parentSlot.currentItem);
        }
    }
}