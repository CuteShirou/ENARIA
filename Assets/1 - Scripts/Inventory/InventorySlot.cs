using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public enum SlotCategory { Inventory, Equipment }
    
     public Image itemImage;
     public Text quantityText;
     public ItemType allowedType = ItemType.None;
     public SlotCategory slotCategory = SlotCategory.Inventory;

    [HideInInspector] public InventoryItem currentItem;
    
    public void SetItem(InventoryItem item)
    {
        currentItem = item;

        if (itemImage == null || quantityText == null)
        {
            Debug.LogWarning("InventorySlot mal configuré !");
            return;
        }

        // Supprime tous les enfants "Inventory Image" sauf le bon
        foreach (Transform child in transform)
        {
            if (child != itemImage.transform && child.name.Contains("Inventory Image"))
            {
                Destroy(child.gameObject);
            }
        }
        
        if (item != null && item.icon != null)
        {
            itemImage.sprite = item.icon;
            itemImage.enabled = true;
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
            quantityText.enabled = item.quantity > 1;
            
            itemImage.transform.SetParent(this.transform);
            itemImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            itemImage.sprite = null;
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
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        var sourceSlot = dragged.parentSlot;
        var draggedItem = sourceSlot.currentItem;

        if (allowedType != ItemType.None && draggedItem.type != allowedType)
        {
            Debug.Log("❌ Ce type d'item ne peut pas être placé ici !");
            return;
        }

        if (slotCategory == SlotCategory.Equipment)
        {
            InventorySlot[] allSlots = FindObjectsOfType<InventorySlot>();
            foreach (var slot in allSlots)
            {
                if (slot == this || slot == sourceSlot) continue;
                if (slot.slotCategory == SlotCategory.Equipment && slot.allowedType == this.allowedType && slot.currentItem != null && slot.currentItem.type == draggedItem.type)
                {
                    Debug.Log("❌ Un item de ce type est déjà équipé !");
                    return;
                }
            }
        }

        if (currentItem != null && currentItem.itemID == draggedItem.itemID && currentItem.quantity < currentItem.maxStack)
        {
            int total = currentItem.quantity + draggedItem.quantity;
            int surplus = Mathf.Max(0, total - currentItem.maxStack);

            currentItem.quantity = Mathf.Min(total, currentItem.maxStack);
            SetItem(currentItem);

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
            if (temp != null)
                sourceSlot.SetItem(temp);
            else
                sourceSlot.ClearSlot();
        }
        
        dragged.parentAfterDrag = this.transform;
    }
}