using UnityEngine;

public class InventorySeeder : MonoBehaviour
{
    [System.Serializable]
    public struct SeedEntry
    {
        public Item item;
        [Min(0)] public int index;
    }

    [Header("Remplissage ciblé (index -> item)")]
    public SeedEntry[] entries;

    [Header("Remplissage automatique (au premier slot vide)")]
    public Item[] autoFillItems;

    [Header("Options")]
    public bool clearBeforeSeeding = false;
    public bool runOnStart = true;

    private void Start()
    {
        if (runOnStart) Run();
    }

    [ContextMenu("Run now")]
    public void Run()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[InventorySeeder] InventoryManager.Instance est nul.");
            return;
        }

        if (clearBeforeSeeding)
        {
            for (int i = 0; i < 1024; i++)
            {
                var existing = InventoryManager.Instance.GetItemAt(i);
                if (existing == null) break;
                InventoryManager.Instance.ClearItemAt(i);
            }
        }
        
        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (e.item == null) continue;
                InventoryManager.Instance.SetItemAt(e.index, e.item);
            }
        }

        if (autoFillItems != null)
        {
            foreach (var it in autoFillItems)
            {
                if (it == null) continue;
                InventoryUtil.AddItemToFirstEmpty(it);
            }
        }
    }
}