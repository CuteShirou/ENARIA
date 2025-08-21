using UnityEngine;
using System.Linq;

public class InventorySlot : MonoBehaviour
{
    [Tooltip("Index de cette case dans la grille.")]
    public int index;

    [Header("Restrictions (facultatif)")]
    [Tooltip("Laisse vide pour accepter tous les types. Sinon, seuls ces types seront autorisés dans ce slot.")]
    public Item.ItemType[] allowedTypes;

    public bool Accepts(Item item)
    {
        if (item == null) return true;
        if (allowedTypes == null || allowedTypes.Length == 0) return true;
        return allowedTypes.Contains(item.itemType);
    }
}