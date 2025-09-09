using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class InventoryContextMenu : MonoBehaviour
{
    [Header("Menu UI")]
    public CanvasGroup menuRoot;
    public Button equipButton;
    public Button unequipButton;
    public Text itemNameText;

    [Header("Behavior")]
    [Tooltip("Décalage par rapport au curseur.")]
    public Vector2 screenOffset = new Vector2(12f, -12f);
    [Tooltip("Fait suivre le curseur tant que le menu est ouvert.")]
    public bool followCursorWhileOpen = false;

    [Header("Events")]
    public ItemSlotEvent onEquip = new ItemSlotEvent();
    public ItemSlotEvent onUnequip = new ItemSlotEvent();
    [System.Serializable] public class ItemSlotEvent : UnityEvent<Item, int> {}
    
    // Public Methodes
    public static InventoryContextMenu Instance { get; private set; }
    public bool IsVisible() => menuRoot && menuRoot.alpha > 0.5f;
    
    // Private methods
    private Item currentItem;
    private int currentSlot = -1;
    private System.Func<Item, int, bool> isEquippedFunc;
    private void HideImmediate() => Hide();
    private void HandleEquip()   { if (currentItem == null) { Hide(); return; } onEquip?.Invoke(currentItem, currentSlot); Hide(); }
    private void HandleUnequip() { if (currentItem == null) { Hide(); return; } onUnequip?.Invoke(currentItem, currentSlot); Hide(); }
    
    void Awake()
    {
        Instance = this;
        HideImmediate();
        if (equipButton)   equipButton.onClick.AddListener(() => { HandleEquip(); });
        if (unequipButton) unequipButton.onClick.AddListener(() => { HandleUnequip(); });
    }

    void OnDisable() { HideImmediate(); }

    void Update()
    {
        if (!IsVisible()) return;

        if (followCursorWhileOpen)
            PositionAtCursor(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition, GetCanvasCamera()))
                Hide();
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) Hide();
    }

    Camera GetCanvasCamera()
    {
        var canvas = GetComponentInParent<Canvas>();
        return canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
    }

    public void ShowFor(Item item, int slotIndex, Vector2 screenPosition, System.Func<Item, int, bool> isEquippedPredicate = null)
    {
        currentItem = item;
        currentSlot = slotIndex;
        isEquippedFunc = isEquippedPredicate;

        if (itemNameText) itemNameText.text = item ? item.itemName : string.Empty;

        bool equipped = false;
        if (isEquippedFunc != null && item != null) equipped = isEquippedFunc(item, slotIndex);

        if (equipButton)   equipButton.gameObject.SetActive(item != null && !equipped);
        if (unequipButton) unequipButton.gameObject.SetActive(item != null &&  equipped);

        Show(); // activer avant pour avoir la bonne taille RectTransform
        PositionAtCursor(screenPosition);
    }

    void PositionAtCursor(Vector2 screenPosition)
    {
        screenPosition += screenOffset;

        var rt = transform as RectTransform;
        if (!rt) return;

        var canvas = GetComponentInParent<Canvas>();
        if (!canvas) return;

        var canvasRect = canvas.transform as RectTransform;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out localPoint))
                rt.anchoredPosition = ClampToCanvas(localPoint, rt, canvasRect);
        }
        else // ScreenSpaceCamera ou WorldSpace
        {
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPosition, canvas.worldCamera, out worldPoint))
            {
                rt.position = worldPoint;

                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvas.worldCamera, out localPoint))
                {
                    var clamped = ClampToCanvas(localPoint, rt, canvasRect);
                    Vector3 clampedWorld;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        canvasRect,
                        RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, canvas.transform.TransformPoint(clamped)),
                        canvas.worldCamera, out clampedWorld
                    );
                    rt.position = clampedWorld;
                }
            }
        }
    }

    Vector2 ClampToCanvas(Vector2 desiredLocalPos, RectTransform rt, RectTransform canvasRect)
    {
        Vector2 size = rt.rect.size;
        Vector2 pivot = rt.pivot;

        float minX = canvasRect.rect.xMin + size.x * pivot.x;
        float maxX = canvasRect.rect.xMax - size.x * (1f - pivot.x);
        float minY = canvasRect.rect.yMin + size.y * pivot.y;
        float maxY = canvasRect.rect.yMax - size.y * (1f - pivot.y);

        desiredLocalPos.x = Mathf.Clamp(desiredLocalPos.x, minX, maxX);
        desiredLocalPos.y = Mathf.Clamp(desiredLocalPos.y, minY, maxY);
        return desiredLocalPos;
    }

    public void Show()
    {
        if (!menuRoot) return;
        gameObject.SetActive(true);
        menuRoot.alpha = 1f;
        menuRoot.blocksRaycasts = true;
        menuRoot.interactable = true;
    }

    public void Hide()
    {
        if (!menuRoot) return;
        menuRoot.alpha = 0f;
        menuRoot.blocksRaycasts = false;
        menuRoot.interactable = false;
        gameObject.SetActive(false);
        currentItem = null;
        currentSlot = -1;
        isEquippedFunc = null;
    }

}
