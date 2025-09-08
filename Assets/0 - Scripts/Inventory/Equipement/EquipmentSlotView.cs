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
        if (!equipment) equipment = FindObjectOfType<EquipmentController>();
        Refresh();
    }

    void OnEnable() { Refresh(); }
    void Update()   { Refresh(); } // simple polling; peut être remplacé par des events

    public void Refresh()
    {
        if (!iconImage && !nameText) return;
        Item cur = equipment ? equipment.GetEquipped(slotType) : null;

        if (iconImage) iconImage.sprite = cur ? cur.icon : emptySprite;
        if (iconImage) iconImage.enabled = (cur ? cur.icon != null : (emptySprite != null));
        if (nameText)  nameText.text = cur ? cur.itemName : string.Empty;
    }

    // Drag depuis inventaire -> équipe si compatible
    public void OnDrop(PointerEventData eventData)
    {
        if (!equipment) return;
        var go = eventData.pointerDrag;
        if (!go) return;

        var sourceView = go.GetComponentInParent<InventorySlotView>();
        if (!sourceView) return;

        var mgr = InventoryManager.Instance;
        if (!mgr) return;

        var item = mgr.GetItemAt(sourceView.Index);
        if (!item) return;
        if (item.itemType != slotType) return;

        equipment.Equip(item, sourceView.Index);
        Refresh();
    }

    // Clic droit sur la case d’équipement -> déséquiper
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
