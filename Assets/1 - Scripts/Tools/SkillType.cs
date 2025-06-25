using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    None,
    Force,
    Dexterité,
    Magie,
    Foi,
}

public enum EffectType
{
    Aucun,
    BonusPV,
    BonusShield,
    BonusPA,
    BonusPM,
    BonusPO,
    MalusPA,
    MalusPM,
    MalusPO,
    BonusFor,
    MalusFor,
    BonusDex,
    MalusDex,
    BonusMag,
    MalusMag,
    BonusFoi,
    MalusFoi,
    BonusResFor,
    MalusResFor,
    BonusResDex,
    MalusResDex,
    BonusResMag,
    MalusResMag,
    BonusResFoi,
    MalusResFoi,
}

[System.Serializable]
public class SkillEffect
{
    public EffectType effectType;
    public float value;
    public int duration;
    public bool applyToSelf;
}

[System.Serializable]
public class ImpactZone
{
    [Tooltip("Coordonnées relatives autour de la case ciblée (0,0).")]
    public Vector2Int[] zone;
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Game Creation Tool/Skill")]
public class SkillData : ScriptableObject
{
    public int ID;
    public string skillName;

    [TextArea(3, 6)]
    public string description;

    [Header("Stats")]
    public int damageMin;
    public int damageMax;
    public SkillType skillType;
    public int costPA;
    public int rangeMin;
    public int rangeMax;
    public int cooldown;
    public int maxPerTargetPerTurn;

    [Header("Zone d’impact personnalisée")]
    public ImpactZone impactZone;

    [Header("Effets Basique")]
    public List<SkillEffect> effects = new List<SkillEffect>();

    [Header("Critique")]
    [Range(0, 100)]
    public float critChance;

    [Header("Effets bonus si Critique")]
    public List<SkillEffect> critEffects = new List<SkillEffect>();
    public Sprite icon;
}

