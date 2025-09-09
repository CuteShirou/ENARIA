using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotView : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Type d'équipement")]
    public Item.ItemType slotType = Item.ItemType.Helmet;

    [Header("UI")]
    public Image iconImage;
    public Text  nameText;
    public Sprite emptySprite;

    [Header("Refs")]
    public EquipmentController equipment;      // auto si vide
    public InventoryContextMenu contextMenu;   // auto si vide

    void Awake()
    {
        if (!equipment) equipment = EquipmentController.Instance ?? FindObjectOfType<EquipmentController>();
        Refresh();
    }

    void OnEnable()
    {
        if (!equipment) equipment = EquipmentController.Instance ?? FindObjectOfType<EquipmentController>();
        if (equipment != null) equipment.OnEquippedChanged += HandleEquippedChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (equipment != null) equipment.OnEquippedChanged -= HandleEquippedChanged;
    }

    void HandleEquippedChanged(Item.ItemType type, Item item)
    {
        if (type == slotType) Refresh();
    }

    public void Refresh()
    {
        if (!iconImage && !nameText) return;
        var cur = equipment ? equipment.GetEquipped(slotType) : null;

        if (iconImage) { iconImage.sprite = cur ? cur.icon : emptySprite; iconImage.enabled = (cur ? cur.icon != null : (emptySprite != null)); }
        if (nameText)  { nameText.text = cur ? cur.itemName : string.Empty; }
    }

    // Drag depuis inventaire -> équipe si compatible
    public void OnDrop(PointerEventData eventData)
    {
        if (!equipment) return;
        var go = eventData.pointerDrag;
        if (!go) return;

        var sourceView = go.GetComponentInParent<InventorySlotView>();
        if (!sourceView) return;

        var mgr = InventoryManager.Instance; // présent dans ton projet
        if (!mgr) return;

        var item = mgr.GetItemAt(sourceView.Index);
        if (!item) return;
        if (item.itemType != slotType) return;

        equipment.Equip(item, sourceView.Index);
        // Refresh() sera appelé via l’événement
    }

    // Clic droit -> déséquiper
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        var cur = equipment ? equipment.GetEquipped(slotType) : null;
        if (!cur) return;

        var menu = contextMenu ? contextMenu : InventoryContextMenu.Instance;
        if (!menu) return;

        menu.ShowFor(cur, -1, eventData.position, (it, _) => true);
    }
}
