using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SkillNode
{
    public SkillData skillData;
    public Sprite manualIcon;
    public string manualName;
    public string manualDescription;
    public int cost;
    public bool isUnlocked;
    public UnityEngine.Events.UnityEvent onUnlock;

    public string SkillName => skillData != null ? skillData.skillName : manualName;
    public string Description => skillData != null ? skillData.description : manualDescription;
    public Sprite Icon => skillData != null ? skillData.icon : manualIcon;

    public string Specifications
    {
        get
        {
            if (skillData == null)
                return "";

            return
                $"PA : {skillData.costPA}" +
                $"    PO : {skillData.rangeMin}-{skillData.rangeMax}\n" +
                $"Critique : {skillData.critChance}%\n" +
                $"Zone : {(skillData.impactZone != null ? skillData.impactZone.zone.Length + " cases" : "–")}\n" +
                $"Relances max/tour : {skillData.maxPerTargetPerTurn}\n" +
                $"Cibles max/tour : {skillData.maxPerTargetPerTurn}\n" +
                $"Cooldown : {skillData.cooldown}";
        }
    }

}
