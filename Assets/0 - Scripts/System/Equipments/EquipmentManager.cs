using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public Entity_StatistiqueCombat combatStats;

    [Header("Équipements Équipés")]
    public EquipmentData coiffe;
    public EquipmentData amulette;
    public EquipmentData plastron;
    public EquipmentData ceinture;
    public EquipmentData jambiere;
    public EquipmentData bottes;
    public EquipmentData cape;
    public EquipmentData arme;
    public EquipmentData bracelet;
    public EquipmentData anneau;
    public EquipmentData gants;

    private List<EquipmentData> AllEquippedItems => new List<EquipmentData>()
    {
        coiffe, amulette, plastron, ceinture, jambiere, bottes,
        cape, arme, bracelet, anneau, gants
    };
    public float[] GetEquipmentBonuses()
    {
        float[] bonuses = new float[14];

        foreach (var item in AllEquippedItems)
        {
            if (item == null) continue;

            bonuses[0] += item.PV;
            bonuses[1] += item.PA;
            bonuses[2] += item.PM;
            bonuses[3] += item.PO;
            bonuses[4] += item.Force;
            bonuses[5] += item.Dexterite;
            bonuses[6] += item.magie;
            bonuses[7] += item.foi;

            bonuses[8] += item.Initiative;
            bonuses[9] += item.critChance;
            bonuses[10] += item.ResForce;
            bonuses[11] += item.ResDexterite;
            bonuses[12] += item.ResMagie;
            bonuses[13] += item.ResFoi;

        }

        return bonuses;
    }
}
