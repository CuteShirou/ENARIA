using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data_Skill
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Game Creation Tool/Skill")]
public class Data_Skill : ScriptableObject
{

    [Header("Identification")]
    public int ID;                          //   ID unique de la compétence
    public string skillName;                //   Nom affiché

    [TextArea(3, 6)]
    public string description;              //   Description utilisateur

    [Header("Classification")]
    public SkillType skillType;             //   Type de compétence (Attack, Boost, Heal, ...)
    public SkillElement skillElement;       //   Élément (Force, Dexterité, Magie, Foi, ...)

    [Header("Main Stats")]
    public int damageMin;                   //   Dégâts min (si Attack)
    public int damageMax;                   //   Dégâts max (si Attack)
    public int costPA;                      //   Coût en PA
    public int rangeMin;                    //   Portée minimale
    public int rangeMax;                    //   Portée maximale
    public int cooldown;                    //   Temps de recharge (tours)
    public int maxPerTargetPerTurn = 99;    //   Lancers max par cible et par tour (99 = quasi illimité)

    [Header("Impact Zone (relative to targeted tile)")]
    public ImpactZone impactZone;           //   Offsets relatifs autour de (0,0) = case ciblée

    [Header("Effects (non-crit)")]
    public List<SkillEffect> effects = new List<SkillEffect>();      //   Effets de base

    [Header("Critical")]
    [Range(0, 100)]
    public float critChance;                //   Chance de critique en %
    public List<SkillEffect> critEffects = new List<SkillEffect>();  //   Effets bonus si critique

    [Header("Animation/FX")]
    public Data_SkillAnimation fxData;   // ScriptableObject décrivant l'anim
    public GameObject fxPrefab;         // Prefab d'FX (particules, animator...)
    public float fxYOffset = 0f;

    [Header("Icon")]
    public Sprite icon;                     //   Icône UI
}
