using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    public void SetIngredient(CraftIngredient ingredient, int playerQuantity)
    {
        if (ingredient == null || ingredient.item == null)
        {
            if (iconImage != null) iconImage.sprite = null;
            if (quantityText != null)
            {
                quantityText.text = "0 / 0";
                quantityText.color = Color.red;
            }
            return;
        }

        if (iconImage != null) iconImage.sprite = ingredient.item.icon;

        int requiredQty = ingredient.quantity;
        if (quantityText != null)
        {
            quantityText.text = $"{playerQuantity} / {requiredQty}";
            quantityText.color = playerQuantity < requiredQty ? Color.red : Color.white;
        }
    }
}
