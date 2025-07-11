using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ActiveEffect
{
    public SkillEffect effect;
    public int turnsRemaining;
    public bool applied; // Pour �viter double application dans le m�me tour

    public ActiveEffect(SkillEffect effect)
    {
        this.effect = effect;
        //this.turnsRemaining = effect.duration;
        this.applied = false;
    }
}

public enum SkillElement
{
    Force,
    Dexterite,
    Magie,
    Foi
}

public class CombatStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseHP = 200;
    public int basePA = 7;
    public int basePM = 4;
    public int basePO = 0;
    public int baseInitiative;
    [Range(0, 100)]
    public float baseCritChance;
    public int baseForce;
    public int baseDexterite;
    public int baseMagie;
    public int baseFoi;

    [Header("R�sistances (en %) de base")]
    [Range(0, 100)]
    public float baseResistanceForce;
    [Range(0, 100)]
    public float baseResistanceDexterite;
    [Range(0, 100)]
    public float baseResistanceMagie;
    [Range(0, 100)]
    public float baseResistanceFoi;


    [Header("Current Stats")]
    public int currentHP;
    public int currentPA;
    public int currentPM;
    public int currentPO;
    public float currentCritChance;
    public int currentForce;
    public int currentDexterite;
    public int currentMagie;
    public int currentFoi;

    [Header("R�sistances (en %)")]
    [Range(0, 100)]
    public float currentResistanceForce;
    [Range(0, 100)]
    public float currentResistanceDexterite;
    [Range(0, 100)]
    public float currentResistanceMagie;
    [Range(0, 100)]
    public float currentResistanceFoi;

    [Header("Effet en cours ")]
    public List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    [Header("Shield")]
    public int currentShield;

    private void Awake()
    {
        currentHP = baseHP;
        currentPA = basePA;
        currentPM = basePM;
        currentPO = basePO;
        currentCritChance = baseCritChance;
        currentForce = baseForce;
        currentDexterite = baseDexterite;
        currentMagie = baseMagie;
        currentFoi = baseFoi;
        currentResistanceForce = baseResistanceForce;
        currentResistanceDexterite = baseResistanceDexterite;
        currentResistanceMagie = baseResistanceMagie;
        currentResistanceFoi = baseResistanceFoi;
    }

    public void ResetTurnStats()
    {
        currentPA = basePA;
        currentPM = basePM;
    }

    // Renvoie la r�sistance � appliquer selon le type du skill
    public float GetResistance(SkillElement element)
    {
        return element switch
        {
            SkillElement.Force => currentResistanceForce,
            SkillElement.Dexterite => currentResistanceDexterite,
            SkillElement.Magie => currentResistanceMagie,
            SkillElement.Foi => currentResistanceFoi,
            _ => 0f,
        };
    }

    // Renvoie la stat offensive � appliquer selon le type du skill
    public int GetStatForType(SkillElement element)
    {
        return element switch
        {
            SkillElement.Force => currentForce,
            SkillElement.Dexterite => currentDexterite,
            SkillElement.Magie => currentMagie,
            SkillElement.Foi => currentFoi,
            _ => 0,
        };
    }
    //public void ApplyInstantEffect(SkillEffect effect)
    //{
    //    int val = Mathf.RoundToInt(effect.value);
    //    switch (effect.effectType)
    //    {
    //        case EffectType.BonusPV: currentHP += val; break;
    //        case EffectType.BonusShield: currentShield += val; break;
    //        case EffectType.BonusPA: currentPA += val; break;
    //        case EffectType.MalusPA: currentPA -= val; break;
    //        case EffectType.BonusPM: currentPM += val; break;
    //        case EffectType.MalusPM: currentPM -= val; break;
    //        case EffectType.BonusPO: currentPO += val; break;
    //        case EffectType.MalusPO: currentPO -= val; break;

    //        case EffectType.BonusFor: currentForce += val; break;
    //        case EffectType.MalusFor: currentForce -= val; break;
    //        case EffectType.BonusDex: currentDexterite += val; break;
    //        case EffectType.MalusDex: currentDexterite -= val; break;
    //        case EffectType.BonusMag: currentMagie += val; break;
    //        case EffectType.MalusMag: currentMagie -= val; break;
    //        case EffectType.BonusFoi: currentFoi += val; break;
    //        case EffectType.MalusFoi: currentFoi -= val; break;

    //        case EffectType.BonusResFor: currentResistanceForce += val; break;
    //        case EffectType.MalusResFor: currentResistanceForce -= val; break;
    //        case EffectType.BonusResDex: currentResistanceDexterite += val; break;
    //        case EffectType.MalusResDex: currentResistanceDexterite -= val; break;
    //        case EffectType.BonusResMag: currentResistanceMagie += val; break;
    //        case EffectType.MalusResMag: currentResistanceMagie -= val; break;
    //        case EffectType.BonusResFoi: currentResistanceFoi += val; break;
    //        case EffectType.MalusResFoi: currentResistanceFoi -= val; break;
    //    }
    //}

    //public void RemoveEffect(SkillEffect effect)
    //{
    //    // M�me logique que ApplyInstantEffect, mais en sens inverse
    //    int val = Mathf.RoundToInt(effect.value);
    //    switch (effect.effectType)
    //    {
    //        case EffectType.BonusShield: currentShield = 0; break;
    //        case EffectType.BonusPA: currentPA -= val; break;
    //        case EffectType.MalusPA: currentPA += val; break;
    //        case EffectType.BonusPM: currentPM -= val; break;
    //        case EffectType.MalusPM: currentPM += val; break;

    //        case EffectType.BonusFor: currentForce -= val; break;
    //        case EffectType.MalusFor: currentForce += val; break;
    //        case EffectType.BonusDex: currentDexterite -= val; break;
    //        case EffectType.MalusDex: currentDexterite += val; break;
    //        case EffectType.BonusMag: currentMagie -= val; break;
    //        case EffectType.MalusMag: currentMagie += val; break;
    //        case EffectType.BonusFoi: currentFoi -= val; break;
    //        case EffectType.MalusFoi: currentFoi += val; break;

    //        case EffectType.BonusResFor: currentResistanceForce -= val; break;
    //        case EffectType.MalusResFor: currentResistanceForce += val; break;
    //        case EffectType.BonusResDex: currentResistanceDexterite -= val; break;
    //        case EffectType.MalusResDex: currentResistanceDexterite += val; break;
    //        case EffectType.BonusResMag: currentResistanceMagie -= val; break;
    //        case EffectType.MalusResMag: currentResistanceMagie += val; break;
    //        case EffectType.BonusResFoi: currentResistanceFoi -= val; break;
    //        case EffectType.MalusResFoi: currentResistanceFoi += val; break;
    //    }
    //}
    //public void UpdateActiveEffects()
    //{
    //    List<ActiveEffect> toRemove = new List<ActiveEffect>();

    //    foreach (var effect in activeEffects)
    //    {
    //        effect.turnsRemaining--;
    //        ApplyInstantEffect(effect.effect);
    //        if (effect.turnsRemaining <= 0)
    //        {
    //            RemoveEffect(effect.effect);
    //            toRemove.Add(effect);
    //        }
    //    }

    //    foreach (var effect in toRemove)
    //        activeEffects.Remove(effect);
    //}
}