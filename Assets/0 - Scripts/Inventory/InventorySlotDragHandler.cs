using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotView))]
public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private InventorySlotView slotView;
    
    private bool isRightHeld = false;
    private Coroutine holdSplitRoutine = null;
    private const float holdInitialDelay = 0.35f;
    private const float holdRepeatInterval = 0.08f;

    private bool TrySplitHalf()
    {
        if (InventoryManager.Instance == null) return false;
        var item = InventoryManager.Instance.GetItemAt(slotView.Index);
        if (item == null) return false;
        if (!InventoryManager.Instance.IsStackable(item)) return false;

        int count = InventoryManager.Instance.GetCountAt(slotView.Index);
        if (count <= 1) return false;

        int amount = count / 2;
        if (amount < 1) amount = 1;

        return InventoryManager.Instance.SplitStack(slotView.Index, amount);
    }

    private bool TrySplitOne()
    {
        if (InventoryManager.Instance == null) return false;
        var item = InventoryManager.Instance.GetItemAt(slotView.Index);
        if (item == null) return false;
        if (!InventoryManager.Instance.IsStackable(item)) return false;

        int count = InventoryManager.Instance.GetCountAt(slotView.Index);
        if (count <= 1) return false;

        return InventoryManager.Instance.SplitStack(slotView.Index, 1);
    }

    private Canvas parentCanvas;
    private RectTransform dragIcon;

    private void Awake()
    {
        slotView = GetComponent<InventorySlotView>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (slotView.Get() == null) return; // rien à drag

        // crée une icône temporaire à suivre
        dragIcon = new GameObject("DragIcon", typeof(RectTransform)).GetComponent<RectTransform>();
        dragIcon.SetParent(parentCanvas.transform, false);
        var image = dragIcon.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = slotView.Get().icon;
        image.raycastTarget = false;
        dragIcon.sizeDelta = new Vector2(64, 64);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var targetSlot = GetComponent<InventorySlotView>();
        var sourceHandler = eventData.pointerDrag?.GetComponent<InventorySlotDragHandler>();

        if (sourceHandler != null && targetSlot != null && sourceHandler.slotView != targetSlot)
        {
            InventoryManager.Instance.SwapItems(sourceHandler.slotView.Index, targetSlot.Index);
        }
    }

    // Simple right-click: split half immediately
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        TrySplitHalf();
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var menu = InventoryContextMenu.Instance;
            if (menu != null)
            {
                var sv = GetComponent<InventorySlotView>();
                var item = sv ? sv.Get() : null;
                var idx  = sv ? sv.Index : -1;

                // Optionnel : détection "équipé" via ton EquipmentController
                var eq = FindObjectOfType<EquipmentController>();
                System.Func<Item, int, bool> pred = (it, _) => eq != null && eq.IsEquipped(it);

                menu.ShowFor(item, idx, eventData.position, pred);

                eventData.Use();   // <- bloque la suite (split)
                return;
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        isRightHeld = true;

        // split half immediately on press
        TrySplitHalf();

        // start hold repeat
        if (holdSplitRoutine != null) StopCoroutine(holdSplitRoutine);
        holdSplitRoutine = StartCoroutine(HoldSplit());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        isRightHeld = false;
        if (holdSplitRoutine != null) { StopCoroutine(holdSplitRoutine); holdSplitRoutine = null; }
    }

    private System.Collections.IEnumerator HoldSplit()
    {
        yield return new WaitForSeconds(holdInitialDelay);
        while (isRightHeld)
        {
            if (!TrySplitOne()) break;
            yield return new WaitForSeconds(holdRepeatInterval);
        }
        holdSplitRoutine = null;
    }

}