using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EquipmentItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text quantityText;
    public TMP_Text priceText;
    public Button buyButton;

    private MarketEntry entry;
    private Action<MarketEntry> onBuy;

    public void Setup(MarketEntry e, Action<MarketEntry> onBuyAction)
    {
        entry = e;
        onBuy = onBuyAction;

        if (icon != null && e.data != null) icon.sprite = e.data.icon;
        if (nameText != null && e.data != null) nameText.text = e.data.equipmentName;
        if (levelText != null && e.data != null) levelText.text = "Niv " + e.data.requiredLevel.ToString();
        if (quantityText != null) quantityText.text = "x" + e.quantity; // affichage de la quantité séparée
        if (priceText != null) priceText.text = e.totalPrice + " K"; // prix total uniquement

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke(entry));
    }
}
