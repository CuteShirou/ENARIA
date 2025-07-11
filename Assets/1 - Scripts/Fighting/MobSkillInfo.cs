using UnityEngine;

[System.Serializable]
public class MobSkillInfo
{
    public string skillName;            // Nom de la compétence
    public SkillType skillType;         // Type de compétence: Attack ? Boost ? Heal ?
    public SkillElement skillElement;   // Element de la compétence:  Force ? Magie ? Dex ? Foi ?
    public int costPA;                  // Cout en PA
    public int rangeMin;                // Portée Minimum de la compétence
    public int rangeMax;                // Portée Maximum de la compétence
    public int damageMin;               // Dégat minimum de la compétence
    public int damageMax;               // Dégat maximum de la compétence
}
