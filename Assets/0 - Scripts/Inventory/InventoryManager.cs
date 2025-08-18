using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> items = new List<Item>();

    [Header("UI")]
    [SerializeField] private Transform ItemContent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite emptySlotSprite;
    
    [Header("Setup")]
    [SerializeField] private int initialSlotCount = 30;

    private Item[] slots;
    private InventorySlotView[] slotViews;
    
    public static Sprite EmptySlotSprite => Instance != null ? Instance.emptySlotSprite : null;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        EnsureSlotsInitialized();
        EnsureDataInitialized();
        RefreshAllSlots();
    }

    private void EnsureSlotsInitialized()
    {
        if (ItemContent == null) { Debug.LogError("[InventoryManager] ItemContent n'est pas assigné."); return; }
        if (slotPrefab == null) { Debug.LogError("[InventoryManager] slotPrefab n'est pas assigné."); return; }
        
        int existing = ItemContent.childCount;
        for (int i = existing; i < initialSlotCount; i++)
        {
            Instantiate(slotPrefab, ItemContent);
        }

        slotViews = ItemContent.GetComponentsInChildren<InventorySlotView>(true);
        for (int i = 0; i < slotViews.Length; i++)
            slotViews[i].BindIndex(i);
    }
    
    private void EnsureDataInitialized()
    {
        if (slots == null || slots.Length != initialSlotCount)
            slots = new Item[initialSlotCount];
    }
    
    public void SetItemAt(int index, Item item)
    {
        if (!IsValid(index)) return;
        slots[index] = item;
        RefreshSlot(index);
    }

    public void ClearItemAt(int index)
    {
        if (!IsValid(index)) return;
        slots[index] = null;
        RefreshSlot(index);
    }

    public Item GetItemAt(int index)
    {
        return IsValid(index) ? slots[index] : null;
    }
    
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
            slotViews[i].Set(slots[i], emptySlotSprite);
    }
    
    public void RefreshSlot(int index)
    {
        if (slotViews == null || !IsValid(index)) return;
        slotViews[index].Set(slots[index], emptySlotSprite);
    }
}