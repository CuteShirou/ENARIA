using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Transform parentAfterDrag;
    public InventorySlot parentSlot;

    private RectTransform rectTransform;
    public CanvasGroup canvasGroup;
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
                // Réduit la pile actuelle
                parentSlot.currentItem.quantity -= 1;
                parentSlot.SetItem(parentSlot.currentItem);

                // Crée un nouvel item de quantité 1
                InventoryItem splitItem = new InventoryItem
                {
                    itemID = parentSlot.currentItem.itemID,
                    icon = parentSlot.currentItem.icon,
                    quantity = 1,
                    maxStack = parentSlot.currentItem.maxStack,
                    type = parentSlot.currentItem.type
                };

                // Crée un clone du visuel
                GameObject clone = Instantiate(gameObject, canvas.transform);
                DraggableItem dragScript = clone.GetComponent<DraggableItem>();

                dragScript.parentSlot = null; // Keep it empty
                dragScript.parentAfterDrag = null;
                dragScript.canvasGroup = clone.GetComponent<CanvasGroup>();

                clone.GetComponent<Image>().sprite = splitItem.icon;
                
                // Initialize item under cursor
                RectTransform rt = clone.GetComponent<RectTransform>();
                rt.position = Input.mousePosition;
                
                // Temporary save item data
                dragScript.GetComponent<DraggableItem>().parentSlot = null;
                dragScript.GetComponent<DraggableItem>().canvasGroup.blocksRaycasts = false;
                
                // Manual drag
                PointerEventData dragEventData = new PointerEventData(EventSystem.current);
                dragEventData.position = Input.mousePosition;
                ExecuteEvents.Execute<IBeginDragHandler>(clone, dragEventData, ExecuteEvents.beginDragHandler);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = GetComponentInParent<InventorySlot>();
        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject target = eventData.pointerEnter;
        InventorySlot targetSlot = target != null ? target.GetComponentInParent<InventorySlot>() : null;

        if (targetSlot == null && parentSlot == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(parentAfterDrag, false);
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;

        if (parentSlot != null)
            parentSlot.SetItem(parentSlot.currentItem);
    }
}