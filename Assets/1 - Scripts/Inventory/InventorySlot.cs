using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image itemImage;

    public void SetItem(Sprite itemSprite)
    {
        itemImage.sprite = itemSprite;
        itemImage.enabled = true;
    }

    public void ClearSlot()
    {
        itemImage.sprite = null;
        itemImage.enabled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                draggedItem.parentAfterDrag = this.transform;
            }
        }
    }
}