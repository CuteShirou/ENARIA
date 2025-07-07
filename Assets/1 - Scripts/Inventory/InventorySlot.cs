using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image itemImage;
    public Text quantityText;

    [HideInInspector]
    public InventoryItem currentItem;

    public void SetItem(InventoryItem item)
    {
        currentItem = item;

        if (item != null)
        {
            itemImage.sprite = item.icon;
            itemImage.enabled = true;
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
            quantityText.enabled = item.quantity > 1;
        }
        else
        {
            itemImage.enabled = false;
            quantityText.text = "";
            quantityText.enabled = false;
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemImage.sprite = null;
        itemImage.enabled = false;
        quantityText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {   
        var dragged = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (dragged == null) return;
        
        var sourceSlot = dragged.parentSlot;
        var draggedItem = sourceSlot.currentItem;

        if (currentItem != null && currentItem.itemID == draggedItem.itemID)
        {
            int total = currentItem.quantity + draggedItem.quantity;
            int surplus = Mathf.Max(0, total - currentItem.maxStack);

            currentItem.quantity = Mathf.Min(total, currentItem.maxStack);
            quantityText.text = currentItem.quantity > 1 ? currentItem.quantity.ToString() : "";

            if (surplus > 0)
            {
                draggedItem.quantity = surplus;
                sourceSlot.SetItem(draggedItem);
            }
            else
            {
                sourceSlot.ClearSlot();
            }
        }
        else
        {
            InventoryItem temp = currentItem;
            SetItem(draggedItem);
            sourceSlot.SetItem(temp);
        }

        dragged.parentAfterDrag = this.transform;
    }
}