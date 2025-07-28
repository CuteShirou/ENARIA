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
                Image img = clone.GetComponent<Image>();
                if (img != null) img.sprite = splitItem.icon;

                // Associe un slot temporaire à ce visuel
                InventorySlot tempSlot = clone.AddComponent<InventorySlot>();
                tempSlot.currentItem = splitItem;
                tempSlot.itemImage = img;
                tempSlot.quantityText = new GameObject("Qty").AddComponent<Text>();
                tempSlot.quantityText.transform.SetParent(clone.transform);
                tempSlot.SetItem(splitItem);

                dragScript.parentSlot = tempSlot;
                dragScript.parentAfterDrag = tempSlot.transform;
                dragScript.canvasGroup = clone.GetComponent<CanvasGroup>();

                // Positionne le visuel sous la souris
                RectTransform rt = clone.GetComponent<RectTransform>();
                rt.position = Input.mousePosition;

                // Lance le drag manuellement
                ExecuteEvents.Execute<IBeginDragHandler>(
                    clone,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.beginDragHandler
                );
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

        if (targetSlot != null)
            parentAfterDrag = targetSlot.transform;
        else
            parentAfterDrag = parentSlot.transform;

        transform.SetParent(parentAfterDrag, false);
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;

        if (parentSlot != null)
            parentSlot.SetItem(parentSlot.currentItem);
    }
}