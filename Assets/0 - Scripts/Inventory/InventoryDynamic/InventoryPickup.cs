using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    public Item item;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        int idx = InventoryUtil.AddItemToFirstEmpty(item);
        if (idx >= 0)
        {
            Debug.Log($"[InventoryPickup] Pickup '{item?.name}' -> slot {idx}. Destruction de l'objet.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[InventoryPickup] Inventaire plein - l'objet reste au sol.");
        }
    }
}