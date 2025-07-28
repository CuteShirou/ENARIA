using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    public void SetIngredient(CraftIngredient ingredient, int playerQuantity)
    {
        if (ingredient.ingredientType == IngredientType.Resource && ingredient.resource != null)
            iconImage.sprite = ingredient.resource.icon;
        else if (ingredient.ingredientType == IngredientType.Equipment && ingredient.equipment != null)
            iconImage.sprite = ingredient.equipment.icon;
        else
            iconImage.sprite = null;

        int requiredQty = ingredient.quantity;
        quantityText.text = $"{playerQuantity} / {requiredQty}";

        if (playerQuantity < requiredQty)
            quantityText.color = Color.red;
        else
            quantityText.color = Color.white;
    }
}
