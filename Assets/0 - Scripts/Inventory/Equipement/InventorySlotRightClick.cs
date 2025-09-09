using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotView))]
public class InventorySlotRightClick : MonoBehaviour, IPointerClickHandler
{
    // Public Methodes
    public InventoryContextMenu contextMenu;
    public bool consumeRightClick = true;
    public bool preferContextMenuOverSplit = true;

    // Private Methodes
    private InventorySlotView slotView;
    private EquipmentController equipmentController;

    void Awake()
    {
        slotView = GetComponent<InventorySlotView>();
        equipmentController = FindObjectOfType<EquipmentController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!preferContextMenuOverSplit) return;

        var item = slotView ? slotView.Get() : null;
        var idx = slotView ? slotView.Index : -1;
        var menu = contextMenu ? contextMenu : InventoryContextMenu.Instance;
        if (!menu) return;

        System.Func<Item, int, bool> predicate = null;
        if (equipmentController != null)
            predicate = (it, _) => equipmentController.IsEquipped(it);

        menu.ShowFor(item, idx, eventData.position, predicate);

        if (consumeRightClick) eventData.Use(); // évite que d'autres handlers prennent le clic droit
    }
}