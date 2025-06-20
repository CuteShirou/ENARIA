using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHP = 200;
    public int maxPA = 7;
    public int maxPM = 4;

    [Header("Current Stats")]
    public int currentHP;
    public int currentPA;
    public int currentPM;

    [Header("Caract�ristiques")]
    public int force;
    public int dexterite;
    public int magie;
    public int foi;

    [Header("Combat Utility")]
    public int initiative;
    public int PO;
    [Range(0, 100)]
    public float critChance;

    [Header("R�sistances (en %)")]
    [Range(0, 100)]
    public float resistanceForce;
    [Range(0, 100)]
    public float resistanceDexterite;
    [Range(0, 100)]
    public float resistanceMagie;
    [Range(0, 100)]
    public float resistanceFoi;

    private void Awake()
    {
        currentHP = maxHP;
        currentPA = maxPA;
        currentPM = maxPM;
    }

    public void ResetTurnStats()
    {
        currentPA = maxPA;
        currentPM = maxPM;
    }

    // Renvoie la r�sistance � appliquer selon le type du skill
    public float GetResistance(SkillType type)
    {
        return type switch
        {
            SkillType.Force => resistanceForce,
            SkillType.Dexterit� => resistanceDexterite,
            SkillType.Magie => resistanceMagie,
            SkillType.Foi => resistanceFoi,
            _ => 0f,
        };
    }

    // Renvoie la stat offensive � appliquer selon le type du skill
    public int GetStatForType(SkillType type)
    {
        return type switch
        {
            SkillType.Force => force,
            SkillType.Dexterit� => dexterite,
            SkillType.Magie => magie,
            SkillType.Foi => foi,
            _ => 0,
        };
    }
}
