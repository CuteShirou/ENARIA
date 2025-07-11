using System.Collections.Generic;
using UnityEngine;

public class DataSkillsMobs : MonoBehaviour
{
    public List<MobSkillInfo> skills = new();

    public MobSkillInfo GetBestSkillInRange(int availablePA, int distance, float targetResistance)
    {
        MobSkillInfo bestSkill = null;
        float bestScore = float.MinValue;

        foreach (var skill in skills)
        {
            if (availablePA < skill.costPA)
                continue;

            if (distance < skill.rangeMin || distance > skill.rangeMax)
                continue;

            float avgDamage = (skill.damageMin + skill.damageMax) / 2f;
            float score = avgDamage * (1 - targetResistance / 100f);

            if (score > bestScore)
            {
                bestScore = score;
                bestSkill = skill;
            }
        }

        return bestSkill;
    }
}
