using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Drop_Loot : MonoBehaviour, IPointerEnterHandler
{
    [Header("Source de l'Item")]
    [SerializeField] private InventoryItemController inventory; // Assigné dans le prefab (même GO)

    [Header("Icône par défaut")]
    [SerializeField] private Sprite defaultIcon; // Utilisée si l'item n'a pas d'icône

    [Header("UI References (Prefab_DropRessource)")]
    [SerializeField] private Image imageRessource; // Image_Ressource

    private Item currentItem;

    private void Start()
    {
        // Récupère l'item depuis InventoryItemController et applique l'icône
        RefreshItemFromInventory();
        ApplyItemVisual();
    }

    // Permet de définir l'item par code en passant PAR le controller (pas de doublon)
    public void SetItem(Item newItem)
    {
        if (inventory != null)
            inventory.AddItem(newItem);

        currentItem = newItem;
        ApplyItemVisual();
    }

    // Récupère l'item courant depuis InventoryItemController
    private void RefreshItemFromInventory()
    {
        currentItem = (inventory != null) ? inventory.GetItem() : null;
    }

    // Met à jour l'icône (item.icon ou defaultIcon)
    private void ApplyItemVisual()
    {
        if (!imageRessource) return;

        Sprite sprite = (currentItem != null && currentItem.icon != null) ? currentItem.icon : defaultIcon;
        imageRessource.sprite = sprite;
        imageRessource.enabled = (sprite != null);
        imageRessource.preserveAspect = true;
    }

    // Affiche la popup avec le nom de l'item
    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshItemFromInventory();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Confort en Editor: auto-référence et aperçu de l'icône
        if (inventory == null)
            inventory = GetComponent<InventoryItemController>();

        RefreshItemFromInventory();
        ApplyItemVisual();
    }
#endif
}
