using UnityEngine;

public enum IngredientType
{
    Resource,
    Equipment
}

[System.Serializable]
public class CraftIngredient
{
    public IngredientType ingredientType;
    public ResourceData resource;
    public EquipmentData equipment;
    public int quantity = 1;
}
