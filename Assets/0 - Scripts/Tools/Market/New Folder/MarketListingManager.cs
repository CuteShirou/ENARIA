using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketListingManager : MonoBehaviour
{
    public static MarketListingManager Instance;

    public List<MarketListing> listings = new List<MarketListing>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Création d'un listing : on retire l'item de l'inventaire (RemoveAmountAt) et on ajoute à la liste locale.
    /// callback indique succès (true) ou échec (false).
    /// </summary>
    public void CreateListing(Item item, int quantity, int price, int sourceIndex, Action<bool> callback)
    {
        if (item == null || quantity <= 0)
        {
            callback?.Invoke(false);
            return;
        }
        if (InventoryManager.Instance == null)
        {
            callback?.Invoke(false);
            return;
        }

        // tentative de suppression
        bool ok = InventoryManager.Instance.RemoveAmountAt(sourceIndex, quantity);
        if (!ok)
        {
            Debug.LogWarning("[MarketListingManager] Impossible de retirer la quantité demandée depuis l'inventaire.");
            callback?.Invoke(false);
            return;
        }

        // save (si tu veux persistance)
        if (InventoryManager.Instance.DebugToggle == null || InventoryManager.Instance.DebugToggle.isOn)
            InventorySaveSystem.Save(InventoryManager.Instance);

        // Ajouter listing local
        var l = new MarketListing { item = item, quantity = quantity, price = price, createdAt = DateTime.UtcNow };
        listings.Add(l);

        Debug.Log($"Listing créé : {quantity}x {item.itemName} pour {price} chacun.");
        callback?.Invoke(true);
    }
}

[Serializable]
public class MarketListing
{
    public Item item;
    public int quantity;
    public int price;
    public DateTime createdAt;
}
