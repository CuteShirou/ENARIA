using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellItemPanel : MonoBehaviour
{
    [Header("UI refs")]
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public TMP_InputField quantityInput;
    public TMP_InputField priceInput;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI feedbackText;

    private Item pendingItem;
    private int pendingSourceIndex;
    private int pendingMaxCount;

    private void Awake()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(ClearPending);
    }

    private void OnDestroy()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(ClearPending);
    }

    /// <summary>
    /// Appelé par SellItemDropHandler quand on drop un item sur le panel.
    /// </summary>
    public void SetPendingItem(Item item, int sourceIndex, int availableCount)
    {
        pendingItem = item;
        pendingSourceIndex = sourceIndex;
        pendingMaxCount = availableCount;

        if (itemImage != null) itemImage.sprite = (item != null) ? item.icon : InventoryManager.EmptySlotSprite;
        if (nameText != null) nameText.text = (item != null) ? item.itemName : "—";

        if (quantityInput != null) quantityInput.text = Mathf.Max(1, availableCount > 0 ? 1 : 0).ToString();
        if (priceInput != null) priceInput.text = "0";

        if (confirmButton != null) confirmButton.interactable = (item != null && availableCount > 0);
        clearFeedback();
    }

    private void clearFeedback()
    {
        if (feedbackText != null) feedbackText.text = string.Empty;
    }

    public void ClearPending()
    {
        pendingItem = null;
        pendingSourceIndex = -1;
        pendingMaxCount = 0;
        if (itemImage != null) itemImage.sprite = InventoryManager.EmptySlotSprite;
        if (nameText != null) nameText.text = "Déposer un item ici";
        if (quantityInput != null) quantityInput.text = "0";
        if (priceInput != null) priceInput.text = "0";
        if (confirmButton != null) confirmButton.interactable = false;
        clearFeedback();
    }

    private void OnConfirm()
    {
        if (pendingItem == null || pendingMaxCount <= 0)
        {
            if (feedbackText != null) feedbackText.text = "Aucun item sélectionné.";
            return;
        }

        int qty = 1;
        if (quantityInput != null) int.TryParse(quantityInput.text, out qty);
        qty = Mathf.Clamp(qty, 1, pendingMaxCount);

        int price = 0;
        if (priceInput != null) int.TryParse(priceInput.text, out price);
        price = Mathf.Max(0, price);

        MarketListingManager.Instance?.CreateListing(pendingItem, qty, price, pendingSourceIndex, success =>
        {
            if (success)
            {
                if (feedbackText != null) feedbackText.text = "Mise en vente créée.";

                // --- Ajout : informer le MarketBuyPanel (affichage du panel d'achat) ---
                var buyPanel = FindObjectOfType<MarketBuyPanel>();
                if (buyPanel != null)
                {
                    var newEntry = new MarketEntry();
                    newEntry.data = pendingItem;
                    newEntry.quantity = qty;
                    newEntry.unitPrice = price;
                    // ne pas écrire totalPrice si c'est une propriété readonly ; il sera calculé si nécessaire
                    buyPanel.AddEntry(newEntry);
                }
                // --- fin ajout ---

                // refresh inventory preview
                var parentView = GetComponentInParent<MarketInventoryView>();
                if (parentView != null) parentView.Refresh();
                else
                {
                    // fallback global
                    var mv = FindObjectOfType<MarketInventoryView>();
                    if (mv != null) mv.Refresh();
                }
                ClearPending();
            }
            else
            {
                if (feedbackText != null) feedbackText.text = "Échec lors de la création du listing.";
            }
        });
    }
}
