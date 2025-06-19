using UnityEngine;

public enum EquipmentType
{
    Coiffe,
    Amulette,
    Plastron,
    Ceinture,
    Jambiere,
    Bottes,
    Cape,
    Arme,
    Bracelet,
    Anneau,
    Gants,
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "Game Creation Tool/Equipment")]
public class EquipmentData : ScriptableObject
{
    public int ID;
    public string equipmentName;
    public EquipmentType type;

    [Header("Stats")]
    public int PV;
    public int PA;
    public int PM;
    public int PO;
    public int Initiative;
    public int Force;
    public int Dexterite;
    public int magie;
    public int foi;

    [Range(0, 100)]
    public float critChance;

    [Range(0, 100)]
    public float ResForce;

    [Range(0, 100)]
    public float ResDexterite;

    [Range(0, 100)]
    public float ResMagie;

    [Range(0, 100)]
    public float ResFoi;

    [TextArea(2, 5)]
    public string description;

    public Sprite icon;
}