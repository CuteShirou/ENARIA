// Skill_Binding.cs
// Élément du SkillBook: associe une compétence (Data_Skill) et son FX d'animation.

using UnityEngine;

[System.Serializable]
public class Skill_Binding
{
    //   Compétence liée à ce slot
    public Data_Skill skill;

    //   Deux façons de définir le FX (au choix) :
    //  1) Fiche d'animation (option A) → recommandé si tu catalogues tes FX
    public Data_SkillAnimation fxData;

    //  2) Ou directement un Prefab Sprite_AnimationRunner (créé par l'outil)
    public Sprite_AnimationRunner fxPrefabOverride;

    //   Ajustement vertical (axe Y) appliqué à la position de la case
    public float fxYOffset = 0f;

}
