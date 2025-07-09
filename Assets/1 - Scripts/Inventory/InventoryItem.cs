using UnityEngine;

public enum ItemType
{
    None,
    Head,
    Chest,
    Gloves,
    Legs,
    Boots,
    Amulette,
    Cape,
    Ceinture,
    Ring,
    Weapon,
    Items,
    Ressources,
}

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public Sprite icon;
    public int quantity;
    public int maxStack;
    public ItemType type;
}
