using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    private CraftUIManager manager;
    private CraftRecipeData recipe;

    public void Initialize(CraftRecipeData recipeData, CraftUIManager mgr)
    {
        recipe = recipeData;
        manager = mgr;

        if (recipe == null || recipe.result == null || recipe.result.item == null)
        {
            SetInvalid();
            return;
        }

        iconImage.sprite = recipe.result.item.icon;
        nameText.text = recipe.result.item.itemName;

        if (levelText != null)
            levelText.text = $"Niv {recipeData.requiredProfessionLevel}";
    }

    private void SetInvalid()
    {
        if (iconImage != null) iconImage.sprite = null;
        if (nameText != null) nameText.text = "Invalide";
    }

    public void OnClick()
    {
        if (manager != null)
            manager.SelectRecipe(recipe);
    }
}
