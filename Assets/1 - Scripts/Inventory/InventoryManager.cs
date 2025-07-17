using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public InventorySlot[] inventorySlots;
    public GameObject draggableItemPrefab;

    void Start()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase non assigné sur " + gameObject.name);
            return;
        }

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("InventorySlots non assignés sur " + gameObject.name);
            return;
        }

        for (int i = 0; i < itemDatabase.items.Count && i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].SetItem(itemDatabase.items[i]);
            else
                Debug.LogWarning("InventorySlot à l'index " + i + " est null.");
        }
    }
}
