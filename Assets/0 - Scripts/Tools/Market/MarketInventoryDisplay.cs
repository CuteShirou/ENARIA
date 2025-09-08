using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketInventoryDisplay : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Transform inventoryPreviewParent; // Le parent dans SellSection où les slots seront instanciés
    [SerializeField] private GameObject sellItemPrefab; // Le prefab à instancier pour chaque item

    private List<GameObject> spawnedItems = new List<GameObject>();

    // Appeler pour mettre à jour la liste
    public void RefreshInventory()
    {
        if (InventoryManager.Instance == null || inventoryPreviewParent == null || sellItemPrefab == null)
        {
            Debug.LogWarning("[MarketInventoryDisplay] References manquantes !");
            return;
        }

        // Vider les anciens items
        foreach (var go in spawnedItems)
        {
            Destroy(go);
        }
        spawnedItems.Clear();

        // Parcourir les slots de l'inventaire
        for (int i = 0; i < InventoryManager.Instance.SlotCapacity; i++)
        {
            var item = InventoryManager.Instance.GetItemAt(i);
            if (item == null) continue; // Pas d'item ici

            int count = InventoryManager.Instance.GetCountAt(i);

            // Instancier le prefab
            var go = Instantiate(sellItemPrefab, inventoryPreviewParent);
            spawnedItems.Add(go);

            // Mettre à jour l'UI du prefab
            var image = go.transform.Find("ItemImage")?.GetComponent<Image>();
            var nameText = go.transform.Find("ItemName")?.GetComponent<Text>();
            var quantityField = go.transform.Find("InputField")?.GetComponent<InputField>();

            if (image != null) image.sprite = item.icon;
            if (nameText != null) nameText.text = item.itemName;
            if (quantityField != null) quantityField.text = count.ToString();

            // Optionnel : tu peux ajouter un bouton de vente pour ce slot
            var sellButton = go.transform.Find("SellButton")?.GetComponent<Button>();
            if (sellButton != null)
            {
                int capturedIndex = i; // nécessaire pour lambda
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() =>
                {
                    int qtyToSell = 1;
                    if (quantityField != null) int.TryParse(quantityField.text, out qtyToSell);
                    SellItem(capturedIndex, qtyToSell);
                });
            }
        }
    }

    private void SellItem(int slotIndex, int quantity)
    {
        if (InventoryManager.Instance == null) return;

        var item = InventoryManager.Instance.GetItemAt(slotIndex);
        if (item == null) return;

        // Retirer l'item du joueur
        InventoryManager.Instance.RemoveAmountAt(slotIndex, quantity);

        Debug.Log($"Vendu {quantity} x {item.itemName} !");
        // Tu peux ici ajouter de l'argent au joueur, etc.

        // Mettre à jour l'affichage
        RefreshInventory();
    }
}
