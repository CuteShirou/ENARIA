using UnityEngine;

public enum ResultType
{
    Resource,
    Equipment
}

[System.Serializable]
public class CraftResult
{
    public ResultType resultType;

    public ResourceData resource;
    public EquipmentData equipment;

    public int quantity = 1;

    public string GetName()
    {
        return resultType == ResultType.Resource
            ? (resource != null ? resource.resourceName : "Invalide")
            : (equipment != null ? equipment.equipmentName : "Invalide");
    }
}
