using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemController : MonoBehaviour
{
    [SerializeField] private Item item;

    public Button RemoveButton;
    
    public void RemoveItem()
    {
        if (item == null) return;
        var slotView = GetComponentInParent<InventorySlotView>();
        if (slotView != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearItemAt(slotView.Index);
            
            if (InventoryManager.Instance.DebugToggle == null || InventoryManager.Instance.DebugToggle.isOn)
                InventorySaveSystem.Save(InventoryManager.Instance);
        }
        item = null;
    }
    
    public void AddItem(Item newItem)
    {
        item = newItem;
    }
}