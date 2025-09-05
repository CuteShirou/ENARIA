using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Game Creation Tool/Crafting Recipe")]
public class CraftRecipeData : ScriptableObject
{
    public enum CraftType
    {
        All,
        Armor,
        Accessories,
        Resources,
        Consumable
    }

    public int requiredProfessionLevel;
    public string recipeName;
    public CraftType craftType = CraftType.All;


    [Header("Ingrédients")]
    public List<CraftIngredient> ingredients;

    [Header("Résultat")]
    public CraftResult result;

    [TextArea]
    public string description;
}















//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(fileName = "CraftRecipe", menuName = "Craft/Recipe")]
//public class CraftRecipeData : ScriptableObject
//{
//    public enum CraftType { All, Weapon, Armor, Consumable }
//    public enum IngredientType { Resource, Equipment }
//    public enum ResultType { Resource, Equipment }

//    [Serializable]
//    public class Ingredient { public IngredientType ingredientType; public int dbId; public int quantity; }

//    public int recipeId;
//    public string recipeName;
//    public CraftType craftType;
//    public int requiredProfessionLevel = 1;
//    public List<Ingredient> ingredients = new List<Ingredient>();

//    [Header("Result")]
//    public ResultType resultType;
//    public int resultDbId;
//    public int resultQuantity = 1;
//}
