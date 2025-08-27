using System;
using System.IO;
using UnityEngine;

[Serializable]

public class InventorySaveData
{
    public int version = 1;
    public int slotCount;
    public int[] itemIds;
    public int[] counts;
}

public static class InventorySaveSystem
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "InventorySave.json");

    public static void Save(InventoryManager mgr)
    {
        if (mgr == null) return;

        int n = mgr.SlotCapacity;
        var data = new InventorySaveData
        {
            slotCount = n,
            itemIds = new int[n],
            counts = new int[n]
        };
        
        for (int i = 0; i < n; i++)
        {
            var it = mgr.GetItemAt(i);
            data.itemIds[i] = (it != null) ? it.id : 0;
            data.counts[i] = mgr.GetCountAt(i);
        }
        
        try
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
            Debug.Log($"[InventorySaveSystem] Saved to {FilePath}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[InventorySaveSystem] Save failed: {e}");
        }
    }
    
    public static void Load(InventoryManager mgr)
    {
        if (mgr == null) return;

        try
        {
            if (!File.Exists(FilePath))
            {
#if UNITY_EDITOR
                Debug.Log("[InventorySaveSystem] No save file found, starting fresh.");
#endif
                return;
            }

            var json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<InventorySaveData>(json);
            if (data == null || data.itemIds == null)
            {
                Debug.LogWarning("[InventorySaveSystem] Save file invalid.");
                return;
            }

            int n = Mathf.Min(mgr.SlotCapacity, data.slotCount > 0 ? data.slotCount : data.itemIds.Length);

            // Prepare arrays at manager capacity and copy what's available
            int[] ids = new int[mgr.SlotCapacity];
            int[] counts = new int[mgr.SlotCapacity];

            for (int i = 0; i < n; i++)
            {
                ids[i] = data.itemIds[i];
                counts[i] = (data.counts != null && i < data.counts.Length) ? data.counts[i] : (ids[i] > 0 ? 1 : 0);
            }

            mgr.SetSuppressAutoSave(true);
            mgr.LoadFrom(ids, counts);
        }
        catch (Exception e)
        {
            Debug.LogError($"[InventorySaveSystem] Load failed: {e}");
        }
        finally
        {
            mgr.SetSuppressAutoSave(false);
        }
    }
}