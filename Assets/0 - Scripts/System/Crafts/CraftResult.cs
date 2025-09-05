using UnityEngine;

public enum ResultType
{
    Resource,
    Equipment
}

[System.Serializable]
public class CraftResult
{
    public Item item;

    public int quantity = 1;

    public string GetName()
    {
        return item != null ? item.itemName : "Invalide";
    }
}
