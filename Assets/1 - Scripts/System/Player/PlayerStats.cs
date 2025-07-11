using UnityEngine;

public enum SpecieType
{
    Humain,
    Elfe
}

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [Header("Informations Générales")]
    public string pseudo;
    public SpecieType specie;
    public int level = 1;
    public int experience = 0;
    public int remainingPoints;

    [Header("Paramètres XP")]
    public int baseXPToLevelUp = 100;
    [Range(1f, 3f)]
    public float xpGrowthFactor = 1.5f;

    public int ExperienceToNextLevel => Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, level - 1));

    private void Start()
    {
        RecalculateLevelFromXP();
        remainingPoints = (level - 1) * 10;
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        while (experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            LevelUp();
        }
    }
    public void RecalculateLevelFromXP()
    {
        int tempLevel = 1;
        int tempXP = experience;

        while (tempXP >= Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, tempLevel - 1)))
        {
            tempXP -= Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, tempLevel - 1));
            tempLevel++;
        }

        level = tempLevel;
        experience = tempXP;
        remainingPoints = (level - 1) * 10;
    }


    private void LevelUp()
    {
        level++;
        remainingPoints += 10;
    }
}
