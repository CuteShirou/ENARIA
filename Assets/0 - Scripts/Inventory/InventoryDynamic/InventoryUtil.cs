using UnityEngine;

public static class InventoryUtil
{
    public static int AddItemToFirstEmpty(Item item)
    {
        if (InventoryManager.Instance == null || item == null) return -1;

        for (int i = 0; i < 1024; i++)
        {
            var existing = InventoryManager.Instance.GetItemAt(i);
            // Si null => libre
            if (existing == null)
            {
                InventoryManager.Instance.SetItemAt(i, item);
                return i;
            }
        }
        return -1;
    }

    public static int AddOrStack(Item item) => AddItemToFirstEmpty(item);

    public static bool RemoveFirst(Item item)
    {
        if (InventoryManager.Instance == null || item == null) return false;
        for (int i = 0; i < 1024; i++)
        {
            var existing = InventoryManager.Instance.GetItemAt(i);
            if (existing == item)
            {
                InventoryManager.Instance.ClearItemAt(i);
                return true;
            }
        }
        return false;
    }
}