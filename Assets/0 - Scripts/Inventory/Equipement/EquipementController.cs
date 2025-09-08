using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    [Header("Links")]
    public InventoryManager inventory; // Assigne dans l’Inspector

    [Header("Config")]
    public Item.ItemType[] equipableTypes = new Item.ItemType[] {
        Item.ItemType.Sword,
        Item.ItemType.Helmet,
        Item.ItemType.Chestplate,
        Item.ItemType.Gloves,
        Item.ItemType.Leggins,
        Item.ItemType.Boots,
        Item.ItemType.Cape,
        Item.ItemType.Ring,
        Item.ItemType.Amulette,
        Item.ItemType.Belt,
        Item.ItemType.Accessory
    };

    // Private Methodes
    private readonly Dictionary<Item.ItemType, Item> equipped = new Dictionary<Item.ItemType, Item>();
    private HashSet<Item.ItemType> _equipableSet;

    void Awake() { _equipableSet = new HashSet<Item.ItemType>(equipableTypes); }

    public bool IsEquipped(Item item)
    {
        if (item == null || !_equipableSet.Contains(item.itemType)) return false;
        return equipped.TryGetValue(item.itemType, out var cur) && cur == item;
    }

    public void Equip(Item item, int fromSlotIndex)
    {
        if (!inventory || item == null || !_equipableSet.Contains(item.itemType)) return;

        if (equipped.TryGetValue(item.itemType, out var prev) && prev && prev != item)
            inventory.AddItem(prev, 1);

        if (fromSlotIndex >= 0) inventory.ClearItemAt(fromSlotIndex);

        equipped[item.itemType] = item;
        Debug.Log($"[EquipmentController] Equipped {item.itemName} in slot {item.itemType}.");
    }

    public void Unequip(Item item, int fromSlotIndex)
    {
        if (!inventory || item == null || !_equipableSet.Contains(item.itemType)) return;

        if (equipped.TryGetValue(item.itemType, out var cur) && cur == item)
        {
            equipped[item.itemType] = null;
            inventory.AddItem(item, 1);
            Debug.Log($"[EquipmentController] Un-equipped {item.itemName} from slot {item.itemType}.");
        }
    }

    public Item GetEquipped(Item.ItemType type)
    {
        equipped.TryGetValue(type, out var cur);
        return cur;
    }
}
