using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventoryGiveButton : MonoBehaviour
{
    public Item itemToGive;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Give);
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(Give);
    }

    private void Give()
    {
        if (itemToGive == null)
        {
            Debug.LogWarning("[InventoryGiveButton] Aucun item assigné.");
            return;
        }

        int idx = InventoryUtil.AddItemToFirstEmpty(itemToGive);
        if (idx >= 0) Debug.Log($"[InventoryGiveButton] Ajout de {itemToGive.name} au slot {idx}.");
        else Debug.LogWarning("[InventoryGiveButton] Inventaire plein.");
    }
}