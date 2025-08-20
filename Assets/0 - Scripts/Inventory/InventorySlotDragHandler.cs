using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotView))]
public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    // Canvas Variable
    private InventorySlotView slotView;
    private Canvas parentCanvas;
    private RectTransform dragIcon;
    private PointerEventData.InputButton lastPointerButton;
    
    // Item variable
    private Item dragItem;
    private Item originalItem;
    private Item draggedItem;
    
    // Int Variables
    private int dragCount;
    private int sourceIndex = -1;
    private int  originalCount;
    private int  draggedCount;
    
    // Bool Variables
    private bool rightClickDrag = false;
    private bool placedSuccessfully = false;

    private void Awake()
    {
        slotView = GetComponent<InventorySlotView>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rightClickDrag = (eventData.button == PointerEventData.InputButton.Right);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        sourceIndex    = slotView.Index;
        originalItem   = InventoryManager.Instance.GetItemAt(sourceIndex);
        originalCount  = InventoryManager.Instance.GetCountAt(sourceIndex);
        placedSuccessfully = false;

        if (originalItem == null) return;

        if (rightClickDrag)
        {
            // Clic droit = tenter de prendre 1 unité si stackable et count > 1
            if (InventoryManager.Instance.IsStackable(originalItem) && originalCount > 1)
            {
                InventoryManager.Instance.RemoveAmountAt(sourceIndex, 1);
                draggedItem  = originalItem;
                draggedCount = 1;
            }
            else
            {
                // sinon, on tombe en drag complet (équivalent clic gauche)
                rightClickDrag = false;
                draggedItem    = originalItem;
                draggedCount   = originalCount;
                InventoryManager.Instance.ClearItemAt(sourceIndex);
            }
        }
        else
        {
            // Clic gauche = drag complet
            draggedItem  = originalItem;
            draggedCount = originalCount;
            InventoryManager.Instance.ClearItemAt(sourceIndex);
        }

        if (draggedItem == null || draggedCount <= 0) return;

        // Crée l’icône de drag (non bloquante)
        dragIcon = new GameObject("DragIcon", typeof(RectTransform)).GetComponent<RectTransform>();
        dragIcon.SetParent(parentCanvas.transform, false);
        var image = dragIcon.gameObject.AddComponent<Image>();
        image.sprite = draggedItem.icon;
        image.raycastTarget = false;
        dragIcon.sizeDelta = new Vector2(64, 64);
    }
    
    private void BeginLeftDrag(PointerEventData eventData)
    {
        CreateDragIcon();
    }

    private void BeginRightDragOne(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null) return;

        sourceIndex = slotView.Index;
        var item = InventoryManager.Instance.GetItemAt(sourceIndex);
        if (item == null) return;

        int count = InventoryManager.Instance.GetCountAt(sourceIndex);
        bool stackable = InventoryManager.Instance.IsStackable(item);
        if (!stackable && count <= 0) return;

        if (!stackable) return;

        bool removed = InventoryManager.Instance.RemoveAmountAt(sourceIndex, 1);
        if (!removed) return;

        dragItem = item;
        dragCount = 1;

        CreateDragIcon();
    }
    
    private void CreateDragIcon()
    {
        if (parentCanvas == null) return;
        if (dragIcon != null) Destroy(dragIcon.gameObject);

        var go = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parentCanvas.transform, false);
        rt.sizeDelta = new Vector2(48, 48);
        dragIcon = rt;

        var img = go.GetComponent<Image>();
        Item iconItem = dragItem != null ? dragItem : slotView.Get();
        var sprite = iconItem != null ? iconItem.icon : InventoryManager.EmptySlotSprite;
        img.sprite = sprite;
        img.raycastTarget = false;

        var cg = go.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.85f;

        UpdateDragIconPosition();
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
        dragIcon = null;

        // Si aucun drop valide n’a eu lieu, on restaure l’état initial côté SOURCE
        if (!placedSuccessfully && draggedItem != null && draggedCount > 0)
        {
            if (rightClickDrag)
            {
                // On avait retiré 1 de la pile : on remet proprement cette unité
                Item current = InventoryManager.Instance.GetItemAt(sourceIndex);
                int  count   = InventoryManager.Instance.GetCountAt(sourceIndex);

                if (current == null)
                    InventoryManager.Instance.SetItemAt(sourceIndex, draggedItem, draggedCount);
                else if (current.id == draggedItem.id && InventoryManager.Instance.IsStackable(draggedItem))
                    InventoryManager.Instance.SetItemAt(sourceIndex, current, count + draggedCount);
                else
                {
                    int empty = InventoryManager.Instance.FindFirstEmpty();
                    if (empty >= 0) InventoryManager.Instance.SetItemAt(empty, draggedItem, draggedCount);
                    else InventoryManager.Instance.Add(draggedItem, draggedCount);
                }
            }
            else
            {
                // Drag complet : restaure exactement la pile d’origine
                InventoryManager.Instance.SetItemAt(sourceIndex, originalItem, originalCount);
            }
        }

        // Reset
        draggedItem = null;
        draggedCount = 0;
        originalItem = null;
        originalCount = 0;
        sourceIndex = -1;
        placedSuccessfully = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var sourceHandler = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventorySlotDragHandler>() : null;
        var targetSlot    = GetComponent<InventorySlotView>();
        if (sourceHandler == null || targetSlot == null) return;

        if (sourceHandler.draggedItem == null || sourceHandler.draggedCount <= 0) return;

        int targetIndex = targetSlot.Index;
        Item targetItem = InventoryManager.Instance.GetItemAt(targetIndex);
        int targetCount = InventoryManager.Instance.GetCountAt(targetIndex);

        // 1) Slot vide
        if (targetItem == null)
        {
            InventoryManager.Instance.SetItemAt(targetIndex, sourceHandler.draggedItem, sourceHandler.draggedCount);
            sourceHandler.draggedCount = 0;
            sourceHandler.placedSuccessfully = true;
            return;
        }

        // 2) Fusion: IMPORTANT — appeler SetItemAt avec **l'INCRÉMENT** uniquement.
        if (targetItem.id == sourceHandler.draggedItem.id && InventoryManager.Instance.IsStackable(sourceHandler.draggedItem))
        {
            int max  = InventoryManager.Instance.MaxStackFor(sourceHandler.draggedItem);
            int room = max - targetCount;
            if (room > 0)
            {
                int add = Mathf.Min(room, sourceHandler.draggedCount);
                // ⚠️ Ne PAS passer targetCount + add, car SetItemAt additionne déjà => double-compte sinon.
                InventoryManager.Instance.SetItemAt(targetIndex, targetItem, add);
                sourceHandler.draggedCount -= add;
                if (sourceHandler.draggedCount == 0)
                {
                    sourceHandler.placedSuccessfully = true;
                }
                else
                {
                    int empty = InventoryManager.Instance.FindFirstEmpty();
                    if (empty >= 0)
                    {
                        InventoryManager.Instance.SetItemAt(empty, sourceHandler.draggedItem, sourceHandler.draggedCount);
                        sourceHandler.draggedCount = 0;
                        sourceHandler.placedSuccessfully = true;
                    }
                }
            }
            return;
        }

        // 3) Item différent
        if (!sourceHandler.rightClickDrag)
        {
            InventoryManager.Instance.SwapItems(sourceHandler.sourceIndex, targetIndex);
            sourceHandler.draggedCount = 0;
            sourceHandler.placedSuccessfully = true;
        }
        else
        {
            int empty = InventoryManager.Instance.FindFirstEmpty();
            if (empty >= 0)
            {
                InventoryManager.Instance.SetItemAt(empty, sourceHandler.draggedItem, sourceHandler.draggedCount);
                sourceHandler.draggedCount = 0;
                sourceHandler.placedSuccessfully = true;
            }
        }
    }
    
    private void PlaceOneOnTarget(int targetIndex)
    {
        var im = InventoryManager.Instance;
        if (im == null || dragItem == null) return;

        var targetItem = im.GetItemAt(targetIndex);
        int targetCount = im.GetCountAt(targetIndex);

        if (targetItem == null)
        {
            im.SetItemAt(targetIndex, dragItem, 1);
            dragCount = 0;
        }
        else if (targetItem.id == dragItem.id && im.IsStackable(dragItem) && targetCount < im.MaxStackFor(dragItem))
        {
            im.SetItemAt(targetIndex, dragItem, 1);
            dragCount = 0;
        }
        else
        {
            TryPlaceAnywhereOrReturn();
        }
    }
    
    private void TryPlaceAnywhereOrReturn()
    {
        var im = InventoryManager.Instance;
        if (im == null || dragItem == null || dragCount <= 0) return;

        // Try to add via generic Add (will top-up then look for empties)
        int remaining = im.Add(dragItem, dragCount);
        dragCount = 0;

        // If nothing could be added (defensive), restore to source slot
        if (remaining < 0) // -1 in our impl means nothing placed
        {
            im.SetItemAt(sourceIndex, dragItem, 1);
        }
    }
    
    private void UpdateDragIconPosition()
    {
        if (dragIcon == null) return;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out pos);
        dragIcon.anchoredPosition = pos;
    }
}