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

        if (recipe == null || recipe.result == null)
        {
            SetInvalid();
            return;
        }

        if (recipe.result.resultType == ResultType.Resource && recipe.result.resource != null)
        {
            iconImage.sprite = recipe.result.resource.icon;
            nameText.text = recipe.result.resource.resourceName;
        }
        else if (recipe.result.resultType == ResultType.Equipment && recipe.result.equipment != null)
        {
            iconImage.sprite = recipe.result.equipment.icon;
            nameText.text = recipe.result.equipment.equipmentName;
        }
        else
        {
            SetInvalid();
        }

        if (levelText != null)
            levelText.text = $"Niv {recipeData.requiredProfessionLevel}";
    }

    private void SetInvalid()
    {
        iconImage.sprite = null;
        nameText.text = "Invalide";
    }

    public void OnClick()
    {
        manager.SelectRecipe(recipe);
    }
}
