using UnityEngine;

public static class InventoryUtilEx
{
    public static int AddAmount(Item item, int amount)
    {
        if (InventoryManager.Instance == null || item == null || amount <= 0) return amount;
        int before = amount;
        InventoryManager.Instance.Add(item, amount);
        
        return 0;
    }
}
