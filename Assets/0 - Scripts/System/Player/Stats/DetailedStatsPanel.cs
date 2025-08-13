using UnityEngine;
using TMPro;

public class DetailedStatsPanel : MonoBehaviour
{
    [Header("Source")]
    public CombatStats combatStats;

    [Header("Colonnes")]
    public TextMeshProUGUI[] equipColumn;
    public TextMeshProUGUI[] tempColumn;
    public TextMeshProUGUI[] totalColumn;

    void Start()
    {
        if (combatStats == null)
        {
            Debug.LogError("CombatStats non assigné !");
            return;
        }

        RefreshPanel();
    }

    public void RefreshPanel()
    {
        float[] equipment = GetEquipmentValues();
        //float[] temporaires = GetTemporaryBonuses();
        float[] baseValues = GetBaseStats();

        for (int i = 0; i < 6; i++)
        {
            equipColumn[i].text = equipment[i].ToString("0.##");
           // tempColumn[i].text = temporaires[i].ToString("0.##");
            totalColumn[i].text = (baseValues[i] + equipment[i] /*+ temporaires[i]*/).ToString("0.##");
        }
    }

    private float[] GetBaseStats()
    {
        return new float[]
        {
            combatStats.baseInitiative,
            combatStats.baseCritChance,
            combatStats.baseResistanceForce,
            combatStats.baseResistanceDexterite,
            combatStats.baseResistanceMagie,
            combatStats.baseResistanceFoi
        };
    }

    private float[] GetEquipmentValues()
    {
        return new float[]
        {
            0,
            0,
            0,
            0,
            0, 
            0
        };
    }

    //private float[] GetTemporaryBonuses()
    //{
    //    float[] bonuses = new float[6];

    //    foreach (var effect in combatStats.activeEffects)
    //    {
    //        if (effect.turnsRemaining <= 0) continue;

    //        var val = effect.effect.value;
    //        switch (effect.effect.effectType)
    //        {
    //            case EffectType.BonusInitiative: bonuses[0] += val; break;
    //            case EffectType.BonusCritChance: bonuses[1] += val; break;
    //            case EffectType.BonusResFor: bonuses[2] += val; break;
    //            case EffectType.BonusResDex: bonuses[3] += val; break;
    //            case EffectType.BonusResMag: bonuses[4] += val; break;
    //            case EffectType.BonusResFoi: bonuses[5] += val; break;
    //        }
    //    }

    //    return bonuses;
    //}
}
