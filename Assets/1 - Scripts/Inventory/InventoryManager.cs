using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public InventorySlot[] inventorySlots;
    public GameObject draggableItemPrefab;

    void Start()
    {
        for (int i = 0; i < itemDatabase.items.Count && i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetItem(itemDatabase.items[i]);
        }
    }
}