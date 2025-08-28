// Skill_Binding.cs
// Élément du SkillBook: associe une compétence (Data_Skill) et son FX d'animation.
// On ne modifie pas Data_Skill : la liaison se fait ici.

using UnityEngine;

[System.Serializable]
public class Skill_Binding
{
    // [FR] Compétence liée à ce slot
    public Data_Skill skill;

    // [FR] Deux façons de définir le FX (au choix) :
    //  1) Fiche d'animation (option A) → recommandé si tu catalogues tes FX
    public Data_SkillAnimation fxData;

    //  2) Ou directement un Prefab Sprite_AnimationRunner (créé par l'outil)
    public Sprite_AnimationRunner fxPrefabOverride;

    // [FR] Ajustement vertical (axe Y) appliqué à la position de la case
    public float fxYOffset = 0f;

    // Constructeur
    public Skill_Binding() { }
    // Déconstructeur
    ~Skill_Binding() { }
}
