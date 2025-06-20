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
    BonusPA,
    BonusPM,
    BonusPO,
    MalusPA,
    MalusPM,
    MalusPO,
    BonusStats,
    MalusStats,
}

[System.Serializable]
public class SkillEffect
{
    public EffectType effectType;
    public float value;
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
    public SkillEffect basicEffect;

    [Header("Critique")]
    [Range(0, 100)]
    public float critChance;

    [Header("Effets bonus si Critique")]
    public SkillEffect critEffect;

    public Sprite icon;
}

