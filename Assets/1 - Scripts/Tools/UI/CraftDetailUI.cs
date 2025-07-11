using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftDetailUI : MonoBehaviour
{
    [Header("Références UI")]
    public TextMeshProUGUI craftNameText;
    public TextMeshProUGUI professionLevelText;
    public Image resultImage;
    public Transform ingredientsContainer;
    public GameObject ingredientSlotPrefab;
    public Button craftButton;
    public GameObject plusImagePrefab;

    private readonly List<GameObject> _ingredientSlots = new List<GameObject>();
    private CraftRecipeData _currentRecipe;

    public void ShowRecipe(CraftRecipeData recipe)
    {
        _currentRecipe = recipe;

        craftNameText.text = recipe.recipeName;
        professionLevelText.text = $"Lv : {recipe.requiredProfessionLevel}";

        if (recipe.result.resultType == ResultType.Resource && recipe.result.resource != null)
            resultImage.sprite = recipe.result.resource.icon;
        else if (recipe.result.resultType == ResultType.Equipment && recipe.result.equipment != null)
            resultImage.sprite = recipe.result.equipment.icon;
        else
            resultImage.sprite = null;

        foreach (var slot in _ingredientSlots)
            Destroy(slot);
        _ingredientSlots.Clear();

        ingredientSlotPrefab.SetActive(false);

        int count = recipe.ingredients.Count;

        if (count == 0)
            return;

        int GetPlayerQuantity(CraftIngredient ing)
        {
            if (CraftManager.Instance == null)
                return 0;

            Object item = ing.ingredientType == IngredientType.Resource ? (Object)ing.resource : (Object)ing.equipment;
            return CraftManager.Instance.GetItemQuantity(item);
        }

        if (count == 1)
        {
            var onlySlotGO = Instantiate(ingredientSlotPrefab, ingredientsContainer);
            onlySlotGO.SetActive(true);
            var onlySlotUI = onlySlotGO.GetComponent<IngredientSlotUI>();
            if (onlySlotUI != null)
                onlySlotUI.SetIngredient(recipe.ingredients[0], GetPlayerQuantity(recipe.ingredients[0]));
            _ingredientSlots.Add(onlySlotGO);
        }
        else
        {
            var firstSlotGO = Instantiate(ingredientSlotPrefab, ingredientsContainer);
            firstSlotGO.SetActive(true);
            var firstSlotUI = firstSlotGO.GetComponent<IngredientSlotUI>();
            if (firstSlotUI != null)
                firstSlotUI.SetIngredient(recipe.ingredients[0], GetPlayerQuantity(recipe.ingredients[0]));
            _ingredientSlots.Add(firstSlotGO);

            if (plusImagePrefab != null)
            {
                var plusGO = Instantiate(plusImagePrefab, ingredientsContainer);
                plusGO.SetActive(true);
                _ingredientSlots.Add(plusGO);
            }

            for (int i = 1; i < count; i += 2)
            {
                var slotGO1 = Instantiate(ingredientSlotPrefab, ingredientsContainer);
                slotGO1.SetActive(true);
                var slotUI1 = slotGO1.GetComponent<IngredientSlotUI>();
                if (slotUI1 != null)
                    slotUI1.SetIngredient(recipe.ingredients[i], GetPlayerQuantity(recipe.ingredients[i]));
                _ingredientSlots.Add(slotGO1);

                if (i + 1 < count)
                {
                    var slotGO2 = Instantiate(ingredientSlotPrefab, ingredientsContainer);
                    slotGO2.SetActive(true);
                    var slotUI2 = slotGO2.GetComponent<IngredientSlotUI>();
                    if (slotUI2 != null)
                        slotUI2.SetIngredient(recipe.ingredients[i + 1], GetPlayerQuantity(recipe.ingredients[i + 1]));
                    _ingredientSlots.Add(slotGO2);
                }

                if (plusImagePrefab != null && i + 2 < count)
                {
                    var plusGO = Instantiate(plusImagePrefab, ingredientsContainer);
                    plusGO.SetActive(true);
                    _ingredientSlots.Add(plusGO);
                }
            }
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftButtonClicked);

        if (CraftManager.Instance != null)
            craftButton.interactable = CraftManager.Instance.CanCraft(_currentRecipe);
        else
            craftButton.interactable = false;
    }

    private void OnCraftButtonClicked()
    {
        if (_currentRecipe == null)
            return;

        if (CraftManager.Instance == null)
        {
            Debug.LogError("CraftManager manquant dans la scène !");
            return;
        }

        if (CraftManager.Instance.Craft(_currentRecipe))
            Debug.Log("Craft terminé !");
        else
            Debug.LogWarning("Impossible de crafter : ressources ou niveau insuffisant !");

        craftButton.interactable = CraftManager.Instance.CanCraft(_currentRecipe);
    }
}
