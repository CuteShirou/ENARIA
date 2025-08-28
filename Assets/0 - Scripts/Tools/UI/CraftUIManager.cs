using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using static CraftRecipeData;

public class CraftUIManager : MonoBehaviour
{
    public CraftDetailUI detailUI;

    [Header("Recettes")]
    public List<CraftRecipeData> recipes;
    public Transform recipeListContent;
    public GameObject recipeItemPrefab;
    public TMP_InputField searchInput;

    private CraftType currentFilter = CraftType.All;
    private string currentSearch = "";

    void Start()
    {
        searchInput.onValueChanged.AddListener(OnSearchChanged);
        PopulateRecipeList();
    }

    void OnSearchChanged(string value)
    {
        currentSearch = value;
        PopulateRecipeList();
    }

    void PopulateRecipeList()
    {
        foreach (Transform child in recipeListContent)
            Destroy(child.gameObject);

        var filtered = recipes.Where(r =>
            (currentFilter == CraftType.All || r.craftType == currentFilter) &&
            (string.IsNullOrEmpty(currentSearch) || r.recipeName.ToLower().Contains(currentSearch.ToLower()))
        );

        foreach (var recipe in filtered)
        {
            GameObject go = Instantiate(recipeItemPrefab, recipeListContent);
            var ui = go.GetComponent<RecipeItemUI>();
            ui.Initialize(recipe, this);
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)recipeListContent);
    }

    public void SelectRecipe(CraftRecipeData recipe)
    {
        detailUI.ShowRecipe(recipe);
    }

    public void SetFilter(int typeIndex)
    {
        currentFilter = (CraftType)typeIndex;
        PopulateRecipeList();
    }
}

















//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;

//public class CraftUIManager : MonoBehaviour
//{
//    public CraftDetailUI detailUI;

//    [Header("Recettes")]
//    public List<CraftRecipeData> recipes;
//    public Transform recipeListContent;
//    public GameObject recipeItemPrefab;
//    public TMP_InputField searchInput;

//    private CraftRecipeData.CraftType currentFilter = CraftRecipeData.CraftType.All;
//    private string currentSearch = "";

//    void Start()
//    {
//        if (searchInput != null)
//            searchInput.onValueChanged.AddListener(OnSearchChanged);
//        PopulateRecipeList();
//    }

//    void OnSearchChanged(string value)
//    {
//        currentSearch = value;
//        PopulateRecipeList();
//    }

//    void PopulateRecipeList()
//    {
//        foreach (Transform child in recipeListContent)
//            Destroy(child.gameObject);

//        var filtered = recipes.Where(r =>
//            (currentFilter == CraftRecipeData.CraftType.All || r.craftType == currentFilter) &&
//            (string.IsNullOrEmpty(currentSearch) || r.recipeName.ToLower().Contains(currentSearch.ToLower()))
//        );

//        foreach (var recipe in filtered)
//        {
//            var go = Instantiate(recipeItemPrefab, recipeListContent);
//            var ui = go.GetComponent<RecipeItemUI>();
//            ui.Initialize(recipe, this);
//        }
//    }

//    public void SelectRecipe(CraftRecipeData recipe)
//    {
//        detailUI.ShowRecipe(recipe);
//    }

//    // Cette méthode déclenche le craft : on délègue tout à CraftManager (server authoritative)
//    public void TryCraft(CraftRecipeData recipe, int quantity = 1)
//    {
//        if (!CraftManager.Instance) { Debug.LogError("CraftManager manquant !"); return; }

//        // Optionnel: vérif locale rapide pour UX
//        if (!CraftManager.Instance.CanCraft(recipe))
//        {
//            Debug.LogWarning("Impossible de crafter : ressources ou niveau insuffisant !");
//            detailUI.ShowRecipe(recipe);
//            return;
//        }

//        // Démarre coroutine d'envoi
//        CraftManager.Instance.StartCraft(recipe, quantity);

//        // On peut afficher feedback UI "en cours..." ici si besoin
//    }

//    public void SetFilter(int typeIndex)
//    {
//        currentFilter = (CraftRecipeData.CraftType)typeIndex;
//        PopulateRecipeList();
//    }
//}
