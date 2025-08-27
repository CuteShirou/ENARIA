using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;

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
    public Button sortByLevelButton;
    public Button sortByQuantityButton;
    public Button sortByPriceButton;

    private string currentSort = "price";
    private bool ascending = true;

    [Header("Buy preview (optional)")]
    public GameObject buyPreviewPanel;
    public TMP_Text previewName;
    public TMP_Text previewLevel;
    public TMP_Text previewPrice;
    public Image previewIcon;
    public Button previewConfirmButton;

    private TypeTabButton currentSelectedTab;
    private EquipmentType? currentFilter = null;

    void Start()
    {
        BuildTabs();

        sortByNameButton.onClick.AddListener(() => ToggleSort("name"));
        sortByLevelButton.onClick.AddListener(() => ToggleSort("level"));
        sortByQuantityButton.onClick.AddListener(() => ToggleSort("quantity"));
        sortByPriceButton.onClick.AddListener(() => ToggleSort("price"));

        ShowItems(null);
        if (buyPreviewPanel != null) buyPreviewPanel.SetActive(false);
    }

    void ToggleSort(string criteria)
    {
        if (currentSort == criteria)
            ascending = !ascending;
        else
        {
            currentSort = criteria;
            ascending = true;
        }

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

        EquipmentType[] order = new EquipmentType[]
        {
            EquipmentType.Coiffe,
            EquipmentType.Amulette,
            EquipmentType.Plastron,
            EquipmentType.Ceinture,
            EquipmentType.Jambiere,
            EquipmentType.Bottes,
            EquipmentType.Cape,
            EquipmentType.Arme,
            EquipmentType.Bracelet,
            EquipmentType.Anneau,
            EquipmentType.Gants
        };

        foreach (var ttype in order)
        {
            var g = Instantiate(tabButtonPrefab, tabsContent);
            var tab = g.GetComponent<TypeTabButton>();
            tab.Init(ttype.ToString(), tt => OnTabSelected(tt), ttype);
        }
    }

    void OnTabSelected(EquipmentType? type)
    {
        foreach (Transform t in tabsContent)
        {
            var tab = t.GetComponent<TypeTabButton>();
            if (tab == null) continue;
            bool selected = (type == null) ? (tab.equipmentType == null) : (tab.equipmentType == type);
            tab.SetSelected(selected);
            if (selected) currentSelectedTab = tab;
        }
        currentFilter = type;
        ShowItems(type);
    }

    void ShowItems(EquipmentType? type)
    {
        ClearChildren(itemsContent);

        var toShow = marketItems.AsEnumerable();
        if (type != null) toShow = toShow.Where(m => m.data != null && m.data.type == type.Value);

        // === TRI ===
        switch (currentSort)
        {
            case "name":
                toShow = ascending ?
                    toShow.OrderBy(m => m.data.equipmentName) :
                    toShow.OrderByDescending(m => m.data.equipmentName);
                break;
            case "level":
                toShow = ascending ?
                    toShow.OrderBy(m => m.data.requiredLevel) :
                    toShow.OrderByDescending(m => m.data.requiredLevel);
                break;
            case "quantity":
                toShow = ascending ?
                    toShow.OrderBy(m => m.quantity) :
                    toShow.OrderByDescending(m => m.quantity);
                break;
            case "price":
                toShow = ascending ?
                    toShow.OrderBy(m => m.unitPrice) :
                    toShow.OrderByDescending(m => m.unitPrice);
                break;
        }

        foreach (var entry in toShow)
        {
            var g = Instantiate(itemPrefab, itemsContent);
            var ui = g.GetComponent<EquipmentItemUI>();
            if (ui != null && entry.data != null) ui.Setup(entry, OnBuyEntry);
        }
    }

    void OnBuyEntry(MarketEntry entry)
    {
        if (buyPreviewPanel == null)
        {
            Debug.Log($"Achat demandé: {entry.data.equipmentName} x{entry.quantity} total={entry.totalPrice}");
            return;
        }

        buyPreviewPanel.SetActive(true);
        if (previewName != null) previewName.text = entry.data.equipmentName;
        if (previewLevel != null) previewLevel.text = "Niv " + entry.data.requiredLevel.ToString();
        if (previewPrice != null) previewPrice.text = entry.totalPrice.ToString() + " K";
        if (previewIcon != null) previewIcon.sprite = entry.data.icon;

        previewConfirmButton.onClick.RemoveAllListeners();
        previewConfirmButton.onClick.AddListener(() => ConfirmBuy(entry));
    }

    void ConfirmBuy(MarketEntry entry)
    {
        Debug.Log($"Confirm achat: {entry.data.equipmentName} x{entry.quantity} total={entry.totalPrice}");
        if (buyPreviewPanel != null) buyPreviewPanel.SetActive(false);
    }

    void ClearChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }
}
