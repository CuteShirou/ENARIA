using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class MarketBuyPanel : MonoBehaviour
{
    [Header("Tabs")]
    public Transform tabsContent;
    public GameObject tabButtonPrefab;

    [Header("Items")]
    public Transform itemsContent;
    public GameObject itemPrefab;

    [Header("Market data")]
    public List<MarketEntry> marketItems = new List<MarketEntry>();

    [Header("Sorting")]
    public Button sortByNameButton;
    public Button sortByValueButton;
    public Button sortByQuantityButton;
    public Button sortByPriceButton;

    private string currentSort = "price";
    private bool ascending = true;

    [Header("Buy preview (optional)")]
    public GameObject buyPreviewPanel;
    public TMP_Text previewName;
    public TMP_Text previewType;
    public TMP_Text previewPrice;
    public Image previewIcon;
    public Button previewConfirmButton;

    [Header("Player / Money")]
    public Entity_Info entityInfo;
    public string currencySuffix = "";

    private TypeTabButton currentSelectedTab;
    private Item.ItemType? currentFilter = null;
    private Dictionary<MarketEntry, bool> firstClickDone = new Dictionary<MarketEntry, bool>();

    void Start()
    {
        BuildTabs();

        if (sortByNameButton != null) sortByNameButton.onClick.AddListener(() => ToggleSort("name"));
        if (sortByValueButton != null) sortByValueButton.onClick.AddListener(() => ToggleSort("value"));
        if (sortByQuantityButton != null) sortByQuantityButton.onClick.AddListener(() => ToggleSort("quantity"));
        if (sortByPriceButton != null) sortByPriceButton.onClick.AddListener(() => ToggleSort("price"));

        if (entityInfo == null)
            entityInfo = FindObjectOfType<Entity_Info>();

        ShowItems(null);
        if (buyPreviewPanel != null) buyPreviewPanel.SetActive(false);
    }

    void ToggleSort(string criteria)
    {
        if (currentSort == criteria) ascending = !ascending;
        else { currentSort = criteria; ascending = true; }
        ShowItems(currentFilter);
    }

    void BuildTabs()
    {
        ClearChildren(tabsContent);

        var goAll = Instantiate(tabButtonPrefab, tabsContent);
        var tabAll = goAll.GetComponent<TypeTabButton>();
        tabAll.Init("Tous", t => OnTabSelected(t), null);
        tabAll.SetSelected(true);
        currentSelectedTab = tabAll;

        Item.ItemType[] order = new Item.ItemType[]
        {
            Item.ItemType.Helmet,
            Item.ItemType.Amulette,
            Item.ItemType.Chestplate,
            Item.ItemType.Belt,
            Item.ItemType.Leggins,
            Item.ItemType.Boots,
            Item.ItemType.Cape,
            Item.ItemType.Sword,
            Item.ItemType.Accessory,
            Item.ItemType.Ring,
            Item.ItemType.Gloves,
            Item.ItemType.Consumable,
            Item.ItemType.Ressource
        };

        foreach (var ttype in order)
        {
            var g = Instantiate(tabButtonPrefab, tabsContent);
            var tab = g.GetComponent<TypeTabButton>();
            tab.Init(ttype.ToString(), tt => OnTabSelected(tt), ttype);
        }
    }

    void OnTabSelected(Item.ItemType? type)
    {
        foreach (Transform t in tabsContent)
        {
            var tab = t.GetComponent<TypeTabButton>();
            if (tab == null) continue;
            bool selected = (type == null) ? (tab.itemType == null) : (tab.itemType == type);
            tab.SetSelected(selected);
            if (selected) currentSelectedTab = tab;
        }
        currentFilter = type;
        ShowItems(type);
    }

    void ShowItems(Item.ItemType? type)
    {
        ClearChildren(itemsContent);

        var toShow = marketItems.AsEnumerable();
        if (type != null) toShow = toShow.Where(m => m.data != null && m.data.itemType == type.Value);

        switch (currentSort)
        {
            case "name":
                toShow = ascending ? toShow.OrderBy(m => m.data.itemName) : toShow.OrderByDescending(m => m.data.itemName);
                break;
            case "value":
                toShow = ascending ? toShow.OrderBy(m => m.data.value) : toShow.OrderByDescending(m => m.data.value);
                break;
            case "quantity":
                toShow = ascending ? toShow.OrderBy(m => m.quantity) : toShow.OrderByDescending(m => m.quantity);
                break;
            case "price":
                toShow = ascending ? toShow.OrderBy(m => m.unitPrice) : toShow.OrderByDescending(m => m.unitPrice);
                break;
        }

        foreach (var entry in toShow)
        {
            var g = Instantiate(itemPrefab, itemsContent);
            var ui = g.GetComponent<ItemMarketUI>();
            if (ui != null && entry.data != null) ui.Setup(entry, OnBuyEntry);
        }
    }

    void OnBuyEntry(MarketEntry entry)
    {
        if (!firstClickDone.ContainsKey(entry) || !firstClickDone[entry])
        {
            firstClickDone[entry] = true;
            Debug.Log($"Premier clic : sélection de {entry.data.itemName}");
            return;
        }

        firstClickDone[entry] = false;
        if (buyPreviewPanel == null)
        {
            Debug.Log($"Achat demandé (pas de preview) : {entry.data.itemName} x{entry.quantity} total={entry.totalPrice}");
            ConfirmBuy(entry);
            return;
        }

        buyPreviewPanel.SetActive(true);
        if (previewName != null) previewName.text = entry.data.itemName;
        if (previewType != null) previewType.text = entry.data.itemType.ToString();

        var formattedTotal = entry.totalPrice.ToString("N0", new CultureInfo("de-DE"));
        var suffix = (entityInfo != null) ? (" " + entityInfo.currencyLabel) : currencySuffix;
        if (previewPrice != null) previewPrice.text = formattedTotal + suffix;
        if (previewIcon != null) previewIcon.sprite = entry.data.icon;

        previewConfirmButton.onClick.RemoveAllListeners();
        previewConfirmButton.onClick.AddListener(() => ConfirmBuy(entry));
    }

    void ConfirmBuy(MarketEntry entry)
    {
        Debug.Log($"Confirm achat: {entry.data.itemName} x{entry.quantity} total={entry.totalPrice}");

        if (entityInfo == null)
        {
            entityInfo = FindObjectOfType<Entity_Info>();
            if (entityInfo == null)
            {
                Debug.LogWarning("Entity_Info introuvable — achat annulé.");
                return;
            }
        }

        long totalPrice = (long)entry.totalPrice;

        if (!entityInfo.TrySpend(totalPrice))
        {
            Debug.LogWarning("Fonds insuffisants — achat annulé.");
            return;
        }

        int remaining = InventoryUtilEx.AddAmount(entry.data, entry.quantity);
        int added = entry.quantity - remaining;

        Debug.Log($"InventoryUtilEx.AddAmount returned remaining={remaining}, added={added}");

        if (remaining > 0)
        {
            long refundAmount = (long)remaining * entry.unitPrice;
            entityInfo.Refund(refundAmount);
            Debug.Log($"Remboursé {refundAmount} au joueur pour les {remaining} items non ajoutés.");
        }

        if (added == entry.quantity)
        {
            marketItems.Remove(entry);
            Debug.Log("Achat complet — items ajoutés à l'inventaire.");
        }
        else if (added > 0)
        {
            entry.quantity = remaining;
            Debug.Log($"Achat partiel — {added} items ajoutés. {entry.quantity} restants sur l'annonce.");
        }
        else
        {
            Debug.LogWarning("Aucun item ajouté — annulation côté client.");
        }

        ShowItems(currentFilter);

        if (buyPreviewPanel != null) buyPreviewPanel.SetActive(false);
    }

    public void AddEntry(MarketEntry entry)
    {
        if (entry == null) return;
        marketItems.Add(entry);
        ShowItems(currentFilter);
    }

    void ClearChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }
}
