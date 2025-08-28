using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data_Skill
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Game Creation Tool/Skill")]
public class Data_Skill : ScriptableObject
{
    // =========================
    // Constructor / Destructor
    // =========================
    public Data_Skill() { /* Constructeur - rien de spécial ici */ }
    ~Data_Skill() { /* Déconstructeur - non utilisé, présent pour respecter la convention */ }

    [Header("Identification")]
    public int ID;                          // [FR] ID unique de la compétence
    public string skillName;                // [FR] Nom affiché

    [TextArea(3, 6)]
    public string description;              // [FR] Description utilisateur

    [Header("Classification")]
    public SkillType skillType;             // [FR] Type de compétence (Attack, Boost, Heal, ...)
    public SkillElement skillElement;       // [FR] Élément (Force, Dexterité, Magie, Foi, ...)

    [Header("Main Stats")]
    public int damageMin;                   // [FR] Dégâts min (si Attack)
    public int damageMax;                   // [FR] Dégâts max (si Attack)
    public int costPA;                      // [FR] Coût en PA
    public int rangeMin;                    // [FR] Portée minimale
    public int rangeMax;                    // [FR] Portée maximale
    public int cooldown;                    // [FR] Temps de recharge (tours)
    public int maxPerTargetPerTurn = 99;    // [FR] Lancers max par cible et par tour (99 = quasi illimité)

    [Header("Impact Zone (relative to targeted tile)")]
    public ImpactZone impactZone;           // [FR] Offsets relatifs autour de (0,0) = case ciblée

    [Header("Effects (non-crit)")]
    public List<SkillEffect> effects = new List<SkillEffect>();      // [FR] Effets de base

    [Header("Critical")]
    [Range(0, 100)]
    public float critChance;                // [FR] Chance de critique en %
    public List<SkillEffect> critEffects = new List<SkillEffect>();  // [FR] Effets bonus si critique

    [Header("Icon")]
    public Sprite icon;                     // [FR] Icône UI
}
