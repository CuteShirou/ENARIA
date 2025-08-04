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
    
     public bool IsValid()
     {
         return itemImage != null && quantityText != null;
     }
     
     public void SetItem(InventoryItem item)
     {
         currentItem = item;

         if (itemImage == null || quantityText == null)
         {
             Debug.LogWarning($"InventorySlot mal configuré : {gameObject.name}");
             return;
         }

         Transform iconContainer = transform.Find("ItemIcon");
         if (iconContainer == null)
         {
             Debug.LogWarning("ItemIcon non trouvé dans ce slot !");
             return;
         }

         // 🔁 Vérifie si l'image est toujours présente dans le bon conteneur
         if (itemImage.transform.parent != iconContainer)
         {
             itemImage.transform.SetParent(iconContainer, false);
         }

         // ✅ Nettoie tous les enfants sauf l'image d'origine et le texte
         foreach (Transform child in iconContainer)
         {
             if (child != itemImage.transform && child != quantityText.transform)
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

             itemImage.rectTransform.anchoredPosition = Vector2.zero;
             itemImage.transform.localScale = Vector3.one;
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
        var draggedItem = dragged.linkedItem ?? sourceSlot?.currentItem;

        if (draggedItem == null)
        {
            Debug.LogWarning("❗ draggedItem est null dans OnDrop");
            return;
        }

        if (slotCategory == SlotCategory.Equipment)
        {
            InventorySlot[] allSlots = Object.FindObjectsByType<InventorySlot>(FindObjectsSortMode.None);
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

            if (sourceSlot != null && sourceSlot.IsValid())
            {
                if (temp != null)
                    sourceSlot.SetItem(temp);
                else
                    sourceSlot.ClearSlot();
            }
        }
        
        dragged.parentAfterDrag = this.transform;
    }
}