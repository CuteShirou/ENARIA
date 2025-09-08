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
        if (recipe == null)
        {
            Debug.LogWarning("ShowRecipe appelé avec une recette nulle.");
            return;
        }

        _currentRecipe = recipe;

        if (craftNameText != null) craftNameText.text = recipe.recipeName;
        if (professionLevelText != null) professionLevelText.text = $"Lv : {recipe.requiredProfessionLevel}";

        if (recipe.result != null && recipe.result.item != null)
            resultImage.sprite = recipe.result.item.icon;
        else
            resultImage.sprite = null;

        foreach (var slot in _ingredientSlots)
            Destroy(slot);
        _ingredientSlots.Clear();

        if (ingredientSlotPrefab != null)
            ingredientSlotPrefab.SetActive(false);

        if (recipe.ingredients == null || recipe.ingredients.Count == 0)
            return;

        int count = recipe.ingredients.Count;

        int GetPlayerQuantity(CraftIngredient ing)
        {
            if (ing == null || ing.item == null) return 0;
            if (CraftManager.Instance == null) return 0;

            Object itemObj = (Object)ing.item;
            return CraftManager.Instance.GetItemQuantity(itemObj);
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
        {
            Debug.Log("Craft terminé !");
            RefreshIngredientDisplay();
        }
        else
        {
            Debug.LogWarning("Impossible de crafter : ressources ou niveau insuffisant !");
        }

        if (CraftManager.Instance != null)
            craftButton.interactable = CraftManager.Instance.CanCraft(_currentRecipe);
    }

    /// Met à jour les quantités affichées dans les IngredientSlotUI existants.
    /// On parcourt _ingredientSlots et on associe dans l'ordre chaque IngredientSlotUI
    /// à l'ingrédient correspondant dans _currentRecipe.ingredients.
    private void RefreshIngredientDisplay()
    {
        if (_currentRecipe == null || _currentRecipe.ingredients == null) return;
        if (_ingredientSlots == null || _ingredientSlots.Count == 0) return;

        int ingIndex = 0;
        for (int i = 0; i < _ingredientSlots.Count && ingIndex < _currentRecipe.ingredients.Count; i++)
        {
            var go = _ingredientSlots[i];
            if (go == null) continue;

            var slotUI = go.GetComponent<IngredientSlotUI>();
            if (slotUI == null) continue;

            var ing = _currentRecipe.ingredients[ingIndex];
            int playerQty = CraftManager.Instance != null ? CraftManager.Instance.GetItemQuantity((Object)ing.item) : 0;
            slotUI.SetIngredient(ing, playerQty);
            ingIndex++;
        }
    }
}













//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class CraftDetailUI : MonoBehaviour
//{
//    [Header("Références UI")]
//    public TextMeshProUGUI craftNameText;
//    public TextMeshProUGUI professionLevelText;
//    public Image resultImage;
//    public Transform ingredientsContainer;
//    public GameObject ingredientSlotPrefab;
//    public Button craftButton;
//    public GameObject plusImagePrefab;

//    private readonly List<GameObject> _ingredientSlots = new();
//    private CraftRecipeData _currentRecipe;

//    public void ShowRecipe(CraftRecipeData recipe)
//    {
//        _currentRecipe = recipe;

//        craftNameText.text = recipe.recipeName;
//        professionLevelText.text = $"Lv : {recipe.requiredProfessionLevel}";

//        resultImage.sprite = recipe.result.resultType switch
//        {
//            ResultType.Resource when recipe.result.resource != null => recipe.result.resource.icon,
//            ResultType.Equipment when recipe.result.equipment != null => recipe.result.equipment.icon,
//            _ => null
//        };

//        foreach (var slot in _ingredientSlots)
//            Destroy(slot);
//        _ingredientSlots.Clear();

//        ingredientSlotPrefab.SetActive(false);

//        int count = recipe.ingredients.Count;
//        if (count == 0) return;

//        int GetPlayerQuantity(CraftIngredient ing)
//        {
//            if (CraftManager.Instance == null)
//                return 0;

//            string id = ing.ingredientType == IngredientType.Resource
//                ? ing.resource.resourceName
//                : ing.equipment.equipmentName;

//            return CraftManager.Instance.GetItemQuantity(id);
//        }

//        if (count == 1)
//        {
//            CreateSlot(recipe.ingredients[0], GetPlayerQuantity(recipe.ingredients[0]));
//        }
//        else
//        {
//            CreateSlot(recipe.ingredients[0], GetPlayerQuantity(recipe.ingredients[0]));

//            if (plusImagePrefab != null)
//                AddPlus();

//            for (int i = 1; i < count; i++)
//            {
//                CreateSlot(recipe.ingredients[i], GetPlayerQuantity(recipe.ingredients[i]));

//                if (plusImagePrefab != null && i < count - 1)
//                    AddPlus();
//            }
//        }

//        craftButton.onClick.RemoveAllListeners();
//        craftButton.onClick.AddListener(OnCraftButtonClicked);
//        craftButton.interactable = CraftManager.Instance?.CanCraft(_currentRecipe) ?? false;
//    }

//    void CreateSlot(CraftIngredient ingredient, int quantity)
//    {
//        var slotGO = Instantiate(ingredientSlotPrefab, ingredientsContainer);
//        slotGO.SetActive(true);
//        var slotUI = slotGO.GetComponent<IngredientSlotUI>();
//        if (slotUI != null)
//            slotUI.SetIngredient(ingredient, quantity);
//        _ingredientSlots.Add(slotGO);
//    }

//    void AddPlus()
//    {
//        var plusGO = Instantiate(plusImagePrefab, ingredientsContainer);
//        plusGO.SetActive(true);
//        _ingredientSlots.Add(plusGO);
//    }

//    private void OnCraftButtonClicked()
//    {
//        if (_currentRecipe == null || CraftManager.Instance == null)
//        {
//            Debug.LogError("CraftManager ou recette manquant !");
//            return;
//        }

//        if (CraftManager.Instance.Craft(_currentRecipe))
//            Debug.Log("Craft terminé !");
//        else
//            Debug.LogWarning("Impossible de crafter : ressources ou niveau insuffisant !");

//        craftButton.interactable = CraftManager.Instance.CanCraft(_currentRecipe);
//    }
//}
