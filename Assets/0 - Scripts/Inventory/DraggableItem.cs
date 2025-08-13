using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Transform parentAfterDrag;
    public InventorySlot parentSlot;

    private RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    private Canvas canvas;
    public InventoryItem linkedItem;

    public void AssociateItem(InventoryItem item)
    {
        linkedItem = item;
    }
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (parentSlot != null && parentSlot.currentItem != null && parentSlot.currentItem.quantity > 1)
            {
                // Réduit la pile actuelle
                parentSlot.currentItem.quantity -= 1;
                parentSlot.SetItem(parentSlot.currentItem);

                // Crée un nouvel item de quantité 1
                InventoryItem splitItem = new InventoryItem
                {
                    itemID = parentSlot.currentItem.itemID,
                    icon = parentSlot.currentItem.icon,
                    quantity = 1,
                    maxStack = parentSlot.currentItem.maxStack,
                    type = parentSlot.currentItem.type
                };

                // Crée un clone visuel de l'item
                GameObject clone = Instantiate(gameObject, canvas.transform);
                DraggableItem dragScript = clone.GetComponent<DraggableItem>();

                dragScript.parentSlot = parentSlot; // Important : conserver la référence source
                dragScript.parentAfterDrag = null;
                dragScript.linkedItem = splitItem; // Attache l'item à manipuler

                dragScript.canvasGroup = clone.GetComponent<CanvasGroup>();
                dragScript.canvasGroup.blocksRaycasts = false;

                // Position initiale du clone
                RectTransform rt = clone.GetComponent<RectTransform>();
                rt.position = Input.mousePosition;

                // Lancer le drag manuellement
                PointerEventData dragEventData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                ExecuteEvents.Execute<IBeginDragHandler>(clone, dragEventData, ExecuteEvents.beginDragHandler);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = GetComponentInParent<InventorySlot>();
        if (parentSlot != null)
        {
            linkedItem = parentSlot.currentItem;
        }

        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject target = eventData.pointerEnter;
        InventorySlot targetSlot = target != null ? target.GetComponentInParent<InventorySlot>() : null;

        // 🔍 Vérifie et réinsère un placeholder si nécessaire dans l'ancien slot
        if (parentSlot != null)
        {
            Transform iconContainer = parentSlot.transform.Find("ItemIcon");
            if (iconContainer != null)
            {
                bool hasPlaceholder = false;
                foreach (Transform child in iconContainer)
                {
                    if (child.name.Contains("Inventory Image"))
                    {
                        hasPlaceholder = true;
                        break;
                    }
                }

                if (!hasPlaceholder && parentSlot.itemImage != null)
                {
                    // Réinstancie un GameObject placeholder visuel si perdu
                    GameObject placeholder = new GameObject("Inventory Image Placeholder", typeof(Image));
                    Image img = placeholder.GetComponent<Image>();
                    img.sprite = parentSlot.itemImage.sprite;
                    img.rectTransform.SetParent(iconContainer, false);
                    img.rectTransform.anchoredPosition = Vector2.zero;
                    img.transform.localScale = Vector3.one;
                    img.raycastTarget = false;
                }
            }
        }

        if (targetSlot == null)
        {
            // Si aucun slot valide n'est ciblé, on remet la quantité dans le slot source
            if (linkedItem != null && parentSlot != null)
            {
                parentSlot.currentItem.quantity += linkedItem.quantity;
                parentSlot.SetItem(parentSlot.currentItem);
            }

            Destroy(gameObject);
            return;
        }

        // Réussite du drop : mise à jour visuelle et logique gérée par InventorySlot.OnDrop()
        transform.SetParent(parentAfterDrag, false);
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;

        // Détruire l'objet temporaire de drag
        Destroy(gameObject);
    }
}