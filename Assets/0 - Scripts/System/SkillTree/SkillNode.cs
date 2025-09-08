using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SkillNode
{
    public SkillData skillData;
    public Sprite manualIcon;
    public string manualName;
    public string manualDescription;
    public int cost;

    [Header("Requirements")]
    [Min(1)]
    public int requiredLevel = 1;

    [Tooltip("Coche ici si la compétence doit démarrer déverrouillée (stocké dans l'asset).")]
    public bool isUnlocked; // flag serialisé dans l'asset (start unlocked)

    [NonSerialized]
    public bool isUnlockedRuntime; // état runtime uniquement (non serialisé)

    public UnityEvent onUnlock;

    // Utiliser cette propriété partout pour savoir si la skill est déverrouillée (start OR runtime)
    public bool IsUnlocked => isUnlocked || isUnlockedRuntime;

    public string SkillName => skillData != null ? skillData.skillName : manualName;
    public string Description => skillData != null ? skillData.description : manualDescription;
    public Sprite Icon => skillData != null ? skillData.icon : manualIcon;

    public string Specifications
    {
        get
        {
            if (skillData == null)
                return "";

            // Calcul intermédiaire pour éviter les quotes échappées dans l'interpolation
            string zoneText = skillData.impactZone != null
                ? skillData.impactZone.zone.Length + " cases"
                : "—";

            return
                $"PA : {skillData.costPA}    PO : {skillData.rangeMin}-{skillData.rangeMax}\n" +
                $"Critique : {skillData.critChance}%\n" +
                $"Zone : {zoneText}\n" +
                $"Relances max/tour : {skillData.maxPerTargetPerTurn}\n" +
                $"Cibles max/tour : {skillData.maxPerTargetPerTurn}\n" +
                $"Cooldown : {skillData.cooldown}";
        }
    }
}
