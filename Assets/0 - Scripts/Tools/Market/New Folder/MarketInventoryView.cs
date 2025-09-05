using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Affiche un aperçu des slots d'InventoryManager dans le Market (Sell section).
/// Le prefab utilisé doit contenir InventorySlotView (pour display) et InventorySlotDragHandler (pour drag).
/// </summary>
public class MarketInventoryView : MonoBehaviour
{
    [Tooltip("Parent (Grid/Content) où instancier les previews")]
    public Transform marketSlotParent;

    [Tooltip("Prefab du slot preview (doit contenir InventorySlotView et InventorySlotDragHandler)")]
    public GameObject marketSlotPrefab;

    private readonly List<GameObject> pool = new List<GameObject>();
    private readonly List<InventorySlotView> activeSlotViews = new List<InventorySlotView>();

    private void OnEnable()
    {
        PopulateFromInventory();
    }

    public void PopulateFromInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[MarketInventoryView] InventoryManager introuvable.");
            return;
        }
        if (marketSlotParent == null || marketSlotPrefab == null)
        {
            Debug.LogWarning("[MarketInventoryView] marketSlotParent ou marketSlotPrefab non assigné.");
            return;
        }

        int cap = InventoryManager.Instance.SlotCapacity;
        EnsurePoolSize(cap);

        // clear previous
        activeSlotViews.Clear();

        for (int i = 0; i < cap; i++)
        {
            var go = pool[i];
            go.transform.SetParent(marketSlotParent, false);
            go.SetActive(true);

            // récupérer le InventorySlotView du prefab
            var view = go.GetComponent<InventorySlotView>();
            if (view == null)
            {
                Debug.LogError("[MarketInventoryView] Le prefab doit contenir InventorySlotView.");
                continue;
            }

            Item item = InventoryManager.Instance.GetItemAt(i);
            int count = InventoryManager.Instance.GetCountAt(i);

            // Binder l'index du view pour que le drag handler puisse connaître l'index source
            view.BindIndex(i);

            // Mettre à jour l'affichage via Set
            view.Set(item, count, InventoryManager.EmptySlotSprite);

            activeSlotViews.Add(view);
        }
    }

    private void EnsurePoolSize(int desired)
    {
        while (pool.Count < desired)
        {
            var go = Instantiate(marketSlotPrefab, marketSlotParent);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    /// <summary>
    /// Met à jour les slots déjà instanciés (utile après craft/vente).
    /// </summary>
    public void Refresh()
    {
        if (InventoryManager.Instance == null) return;
        int cap = InventoryManager.Instance.SlotCapacity;

        for (int i = 0; i < activeSlotViews.Count && i < cap; i++)
        {
            var view = activeSlotViews[i];
            if (view == null) continue;
            int idx = view.Index;
            var item = InventoryManager.Instance.GetItemAt(idx);
            var cnt = InventoryManager.Instance.GetCountAt(idx);
            view.Set(item, cnt, InventoryManager.EmptySlotSprite);
        }
    }
}
