
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
    public Toggle DebugToggle;

    [Header("Save")]
    [Tooltip("Sauvegarder automatiquement à chaque modification.")]
    [SerializeField] private bool autoSaveOnChange = true;

    // PUBLIC VARIABLES
    public static InventoryManager Instance;
    public List<Item> items = new List<Item>();
    public static Sprite EmptySlotSprite => Instance != null ? Instance.emptySlotSprite : null;

    // PRIVATE VARIABLES
    private Item[] slots;
    private int[] counts;
    private InventorySlotView[] slotViews;
    private bool suppressAutoSave = false;

    private const int DEFAULT_MAX_STACK_CONSUMABLE = 99;
    
    public int SlotCapacity => initialSlotCount;

    private void Awake() => Instance = this;

    private void Start()
    {
        EnsureSlotsInitialized();
        EnsureDataInitialized();
        EnsureCanonicalItemIds();
        RefreshAllSlots();
        // Optionnel : laisser InventorySaveSystem décider du load initial
    }

    private void EnsureCanonicalItemIds()
    {
        if (items == null) return;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null) items[i].id = i + 1; // 1-based
        }
    }
    
    private int GetOrAssignCanonicalId(Item item)
    {
        if (item == null) return 0;
        
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

    // ------ STACK RULES ------
    // Seuls les consommables sont empilables.
    public bool IsStackable(Item item) => item != null && item.itemType == Item.ItemType.Consumable || item.itemType == Item.ItemType.Ressource;
    public int MaxStackFor(Item item) => IsStackable(item) ? DEFAULT_MAX_STACK_CONSUMABLE : 1;
    // -------------------------
    
    public int FindFirstEmpty()
    {
        for (int i = 0; i < initialSlotCount; i++)
            if (slots[i] == null) return i;
        return -1;
    }

    public int FindFirstStackable(Item item)
    {
        if (item == null) return -1;
        for (int i = 0; i < initialSlotCount; i++)
        {
            if (slots[i] != null && slots[i].id == item.id && IsStackable(item) && counts[i] < MaxStackFor(item))
                return i;
        }
        return -1;
    }

    public int  GetCountAt(int index) => IsValid(index) ? counts[index] : 0;

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
        SaveIfNeeded();
        return true;
    }

    // Ajout unitaire
    public int AddItem(Item item, int amount = 1) => Add(item, amount);

    // Ajout "bulk" : remplit piles existantes puis cases vides, clamp MaxStack
    public int Add(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return -1;
        NormalizeItemId(item);

        int remaining = amount;
        int firstIndexUsed = -1;

        // 1) Remplir les piles existantes (consommables seulement)
        if (IsStackable(item))
        {
            for (int i = 0; i < initialSlotCount && remaining > 0; i++)
            {
                if (slots[i] != null && slots[i].id == item.id)
                {
                    int max = MaxStackFor(item);
                    int room = max - counts[i];
                    if (room > 0)
                    {
                        int add = Mathf.Min(room, remaining);
                        counts[i] += add;
                        remaining -= add;
                        if (firstIndexUsed < 0) firstIndexUsed = i;
                        RefreshSlot(i);
                    }
                }
            }
        }

        // 2) Utiliser les cases vides
        for (int i = 0; i < initialSlotCount && remaining > 0; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                if (IsStackable(item))
                {
                    int max = MaxStackFor(item);
                    int add = Mathf.Min(max, remaining);
                    counts[i] = add;
                    remaining -= add;
                }
                else
                {
                    counts[i] = 1;
                    remaining -= 1;
                }
                if (firstIndexUsed < 0) firstIndexUsed = i;
                RefreshSlot(i);
            }
        }

        SaveIfNeeded();
        return firstIndexUsed;
    }

    public void SetItemAt(int index, Item item) => SetItemAt(index, item, (item != null) ? 1 : 0);

    public void SetItemAt(int index, Item item, int count)
    {
        if (!IsValid(index)) return;
        if (item != null) NormalizeItemId(item);

        // Fusion si même item et empilable
        if (item != null && IsStackable(item) && slots[index] != null && slots[index].id == item.id)
        {
            int max = MaxStackFor(item);
            counts[index] = Mathf.Clamp(counts[index] + Mathf.Max(1, count), 0, max);
            RefreshSlot(index);
            SaveIfNeeded();
            return;
        }

        slots[index] = item;
        if (item == null)
        {
            counts[index] = 0;
        }
        else
        {
            counts[index] = Mathf.Max(0, count);
            if (!IsStackable(item)) counts[index] = 1;
            int max = MaxStackFor(item);
            if (IsStackable(item) && counts[index] > max) counts[index] = max;
        }
        RefreshSlot(index);
        SaveIfNeeded();
    }

    public void ClearItemAt(int index)
    {
        if (!IsValid(index)) return;
        slots[index] = null;
        counts[index] = 0;
        RefreshSlot(index);
        SaveIfNeeded();
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
        SaveIfNeeded();
    }

    private bool IsValid(int index) => index >= 0 && index < initialSlotCount;

    private void RefreshAllSlots()
    {
        for (int i = 0; i < initialSlotCount; i++)
            RefreshSlot(i);
    }

    private void RefreshSlot(int index)
    {
        if (slotViews == null || !IsValid(index)) return;
        slotViews[index].Set(slots[index], counts[index], emptySlotSprite);
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (!IsValid(indexA) || !IsValid(indexB)) return;

        var a = slots[indexA];
        var b = slots[indexB];
        if (a != null && b != null && a.id == b.id && IsStackable(a))
        {
            int max = MaxStackFor(a);
            int room = max - counts[indexB];
            if (room > 0)
            {
                int move = Mathf.Min(room, counts[indexA]);
                counts[indexB] += move;
                counts[indexA] -= move;
                if (counts[indexA] == 0) slots[indexA] = null;
                RefreshSlot(indexA);
                RefreshSlot(indexB);
                SaveIfNeeded();
                return;
            }
        }

        var tmpItem = slots[indexA];
        var tmpCount = counts[indexA];
        slots[indexA] = slots[indexB];
        counts[indexA] = counts[indexB];
        slots[indexB] = tmpItem;
        counts[indexB] = tmpCount;

        RefreshSlot(indexA);
        RefreshSlot(indexB);
        SaveIfNeeded();
    }

    public bool SplitStack(int index, int amount)
    {
        if (!IsValid(index) || amount <= 0) return false;
        var item = slots[index];
        if (item == null || !IsStackable(item)) return false;
        if (counts[index] <= amount) return false;

        int empty = FindFirstEmpty();
        if (empty < 0) return false;

        slots[empty] = item;
        counts[empty] = amount;
        counts[index] -= amount;

        RefreshSlot(index);
        RefreshSlot(empty);
        SaveIfNeeded();
        return true;
    }

    // ----------- SAVE / LOAD ------------
    public void SetSuppressAutoSave(bool value) => suppressAutoSave = value;

    private void SaveIfNeeded()
    {
        if (autoSaveOnChange && !suppressAutoSave)
        {
            InventorySaveSystem.Save(this);
        }
    }

    // Reçoit des IDs (1-based) + counts et charge l'état
    public void LoadFrom(int[] ids, int[] loadedCounts)
    {
        EnsureDataInitialized();
        EnsureCanonicalItemIds();

        int n = Mathf.Min(initialSlotCount, ids != null ? ids.Length : 0);

        for (int i = 0; i < initialSlotCount; i++)
        {
            Item it = null;
            int c = 0;

            if (i < n)
            {
                int id = ids[i];
                if (id > 0 && id <= items.Count)
                {
                    it = items[id - 1];
                    NormalizeItemId(it); // S'assure que l'ID correspond
                }

                if (it != null)
                {
                    if (IsStackable(it))
                    {
                        int loaded = (loadedCounts != null && i < loadedCounts.Length) ? loadedCounts[i] : 1;
                        c = Mathf.Clamp(loaded, 1, MaxStackFor(it));
                    }
                    else
                    {
                        c = 1;
                    }
                }
            }

            slots[i] = it;
            counts[i] = c;
            RefreshSlot(i);
        }
    }

    // ---------- DEBUG UI TOGGLES (restaurés) ----------
    public void EnableItemRemover()
    {
        if (ItemContent == null) return;

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
