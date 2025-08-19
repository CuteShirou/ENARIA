using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform ItemContent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite emptySlotSprite;
    
    [Header("Setup")]
    [SerializeField] private int initialSlotCount = 30;

    [Header("Debug")]
    [SerializeField] private GameObject PageDebug;

    // PUBLIC VARIABLES
    public static InventoryManager Instance;
    public List<Item> items = new List<Item>();
    public static Sprite EmptySlotSprite => Instance != null ? Instance.emptySlotSprite : null;
    public Toggle DebugToggle;

    // PRIVATE VARIABLES
    private Item[] slots;
    private int[] counts;
    private InventorySlotView[] slotViews;

    private void Awake() => Instance = this;

    private void Start()
    {
        EnsureSlotsInitialized();
        EnsureDataInitialized();
        EnsureCanonicalItemIds();
        RefreshAllSlots();
    }

    private void EnsureCanonicalItemIds()
    {
        if (items == null) return;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it == null) continue;
            int canonical = i + 1;
            if (it.id != canonical)
                it.id = canonical;
        }
    }
    
    private int GetOrAssignCanonicalId(Item item)
    {
        if (item == null) return 0;
        // si déjà dans la liste -> id = index+1
        int idx = items.IndexOf(item);
        if (idx >= 0)
        {
            int canonical = idx + 1;
            if (item.id != canonical) item.id = canonical;
            return canonical;
        }
        
        items.Add(item);
        int newId = items.Count;
        item.id = newId;
        return newId;
    }

    private void NormalizeItemId(Item item)
    {
        if (item == null) return;
        GetOrAssignCanonicalId(item);
    }

    private void EnsureSlotsInitialized()
    {
        if (ItemContent == null) { Debug.LogError("[InventoryManager] ItemContent n'est pas assigné."); return; }
        if (slotPrefab == null) { Debug.LogError("[InventoryManager] slotPrefab n'est pas assigné."); return; }

        int existing = ItemContent.childCount;
        for (int i = existing; i < initialSlotCount; i++)
            Instantiate(slotPrefab, ItemContent);

        slotViews = ItemContent.GetComponentsInChildren<InventorySlotView>(true);
        for (int i = 0; i < slotViews.Length; i++)
            slotViews[i].BindIndex(i);
    }

    private void EnsureDataInitialized()
    {
        if (slots == null || slots.Length != initialSlotCount) slots = new Item[initialSlotCount];
        if (counts == null || counts.Length != initialSlotCount) counts = new int[initialSlotCount];
    }

    public bool IsStackable(Item item) => (item != null && item.itemType == Item.ItemType.Consumable);

    public int FindFirstEmpty()
    {
        for (int i = 0; i < initialSlotCount; i++)
            if (slots[i] == null) return i;
        return -1;
    }

    public int FindFirstStackableSlot(Item item)
    {
        if (!IsStackable(item)) return -1;
        NormalizeItemId(item);
        for (int i = 0; i < initialSlotCount; i++)
            if (slots[i] != null && slots[i].itemType == Item.ItemType.Consumable && slots[i].id == item.id)
                return i;
        return -1;
    }

    public int GetCountAt(int index) => IsValid(index) ? counts[index] : 0;

    public bool RemoveAmountAt(int index, int amount = 1)
    {
        if (!IsValid(index) || amount <= 0) return false;
        if (slots[index] == null) return false;

        if (IsStackable(slots[index]))
        {
            counts[index] -= amount;
            if (counts[index] <= 0)
            {
                slots[index] = null;
                counts[index] = 0;
            }
        }
        else
        {
            slots[index] = null;
            counts[index] = 0;
        }
        RefreshSlot(index);
        return true;
    }

    public int AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return -1;
        NormalizeItemId(item);

        if (IsStackable(item))
        {
            int idx = FindFirstStackableSlot(item);
            if (idx >= 0)
            {
                counts[idx] += amount;
                RefreshSlot(idx);
                return idx;
            }
        }

        int empty = FindFirstEmpty();
        if (empty >= 0)
        {
            slots[empty] = item;
            counts[empty] = IsStackable(item) ? amount : 1;
            RefreshSlot(empty);
            return empty;
        }
        return -1;
    }

    public void SetItemAt(int index, Item item)
    {
        SetItemAt(index, item, (item != null) ? 1 : 0);
    }

    public void SetItemAt(int index, Item item, int count)
    {
        if (!IsValid(index)) return;
        if (item != null) NormalizeItemId(item);

        if (item != null && IsStackable(item) && slots[index] != null && slots[index].id == item.id)
        {
            counts[index] += Mathf.Max(1, count);
            RefreshSlot(index);
            return;
        }

        slots[index] = item;
        counts[index] = Mathf.Max(0, count);
        if (item == null) counts[index] = 0;
        if (item != null && !IsStackable(item)) counts[index] = 1;
        RefreshSlot(index);
    }

    public void ClearItemAt(int index)
    {
        if (!IsValid(index)) return;
        slots[index] = null;
        counts[index] = 0;
        RefreshSlot(index);
    }

    public Item GetItemAt(int index) => IsValid(index) ? slots[index] : null;

    public void Remove(Item item)
    {
        if (item == null || slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == item)
            {
                ClearItemAt(i);
                break;
            }
        }
    }

    private bool IsValid(int index) => index >= 0 && index < initialSlotCount;

    public void RefreshAllSlots()
    {
        if (slotViews == null) return;
        for (int i = 0; i < slotViews.Length && i < slots.Length; i++)
            slotViews[i].Set(slots[i], counts[i], emptySlotSprite);
    }

    public void RefreshSlot(int index)
    {
        if (slotViews == null || !IsValid(index)) return;
        slotViews[index].Set(slots[index], counts[index], emptySlotSprite);
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (!IsValid(indexA) || !IsValid(indexB)) return;

        var tmpItem = slots[indexA];
        var tmpCount = counts[indexA];
        slots[indexA] = slots[indexB];
        counts[indexA] = counts[indexB];
        slots[indexB] = tmpItem;
        counts[indexB] = tmpCount;

        RefreshSlot(indexA);
        RefreshSlot(indexB);
    }

    public void EnableItemRemover()
    {
        if (DebugToggle != null && DebugToggle.isOn)
        {
            foreach (Transform item in ItemContent)
            {
                var btn = item.Find("DebugButton");
                if (btn != null) btn.gameObject.SetActive(true);
            }
        }
        else 
        {
            foreach (Transform item in ItemContent)
            {
                var btn = item.Find("DebugButton");
                if (btn != null) btn.gameObject.SetActive(false);
            }
        }
    }

    public void EnableDebugMenu()
    {
        if (PageDebug == null) return;
        if (DebugToggle != null && DebugToggle.isOn) PageDebug.SetActive(true);
        else PageDebug.SetActive(false);
    }
}
