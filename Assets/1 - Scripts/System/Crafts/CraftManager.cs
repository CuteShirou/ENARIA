using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    public static CraftManager Instance;

    public int playerProfessionLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanCraft(CraftRecipeData recipe)
    {
        if (playerProfessionLevel < recipe.requiredProfessionLevel)
            return false;

        // Vérifier l'inventaire réel (à implémenter)
        return true; // provisoire
    }

    public bool Craft(CraftRecipeData recipe)
    {
        if (!CanCraft(recipe))
            return false;

        // Consommer les ingrédients dans l'inventaire réel

        // Ajouter le résultat dans l'inventaire réel

        Debug.Log($"Craft réussi ! Résultat : {recipe.result}");

        return true;
    }

    public int GetItemQuantity(Object item)
    {

        return 0;
    }

}
