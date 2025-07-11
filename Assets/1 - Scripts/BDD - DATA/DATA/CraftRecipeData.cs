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
        Ressources,
        Consomable
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
