using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SkillNode
{
    public Data_Skill skillData;
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

    // ---------------------------
    // Upgrade explicit (référencé par la node)
    // Si targetSkill est set, on cherchera la skill correspondante dans le SkillBook et on appliquera les overrides.
    // Les champs "overrideX" indiquent si on doit remplacer la valeur runtime.
    // ---------------------------

    [Header("Upgrade (optional)")]
    [Tooltip("Référence à la skill (Data_Skill) que cette node améliorera. Si null => cette node est un débloqueur de 'skillData'.")]
    public Data_Skill targetSkill;

    [Tooltip("Remplacements appliqués à la skill cible (runtime only). Cocher le champ correspondant pour remplacer sa valeur.")]
    public SkillUpgrade upgrade = new SkillUpgrade();

    [Serializable]
    public class SkillUpgrade
    {
        // Identification (optionnel)
        public bool overrideName;
        public string skillName;

        public bool overrideDescription;
        [TextArea(2, 4)] public string description;

        // Classification
        public bool overrideSkillType;
        public SkillType skillType;
        public bool overrideSkillElement;
        public SkillElement skillElement;

        // Main stats (ints)
        public bool overrideDamageMin;
        public int damageMin;
        public bool overrideDamageMax;
        public int damageMax;

        public bool overrideCostPA;
        public int costPA;

        public bool overrideRangeMin;
        public int rangeMin;
        public bool overrideRangeMax;
        public int rangeMax;

        public bool overrideCooldown;
        public int cooldown;

        public bool overrideMaxPerTargetPerTurn;
        public int maxPerTargetPerTurn;

        // ImpactZone (reference)
        public bool overrideImpactZone;
        public ImpactZone impactZone;

        // Effects (remplace la liste entière si coché)
        public bool overrideEffects;
        public List<SkillEffect> effects = new List<SkillEffect>();

        // Critical
        public bool overrideCritChance;
        public float critChance;
        public bool overrideCritEffects;
        public List<SkillEffect> critEffects = new List<SkillEffect>();

        // Icon
        public bool overrideIcon;
        public Sprite icon;

        // Utilitaire : retourne vrai si au moins un override est activé
        public bool HasAnyOverride()
        {
            // check manually to avoid reflection
            return overrideName || overrideDescription ||
                   overrideSkillType || overrideSkillElement ||
                   overrideDamageMin || overrideDamageMax ||
                   overrideCostPA ||
                   overrideRangeMin || overrideRangeMax ||
                   overrideCooldown || overrideMaxPerTargetPerTurn ||
                   overrideImpactZone || overrideEffects ||
                   overrideCritChance || overrideCritEffects ||
                   overrideIcon;
        }
    }

    public string Specifications
    {
        get
        {
            if (skillData == null)
                return "";

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
