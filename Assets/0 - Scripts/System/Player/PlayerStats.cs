using UnityEngine;
using UnityEngine.Events;


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

    [Header("Monnaie")]
    [Tooltip("Montant d'argent courant du joueur.")]
    public long gold;
    [Tooltip("Optionnel : nom de la devise affichée dans les UI.")]
    public string currencyLabel = "";

    public UnityEvent<long> OnGoldChanged;

    private void Start()
    {
        RecalculateLevelFromXP();
        remainingPoints = (level - 1) * 10;

        if (OnGoldChanged == null)
            OnGoldChanged = new UnityEvent<long>();

        OnGoldChanged.Invoke(gold);
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

    public bool CanAfford(long amount)
    {
        return amount <= gold;
    }

    public bool TrySpend(long amount)
    {
        if (amount <= 0) return true;
        if (!CanAfford(amount)) return false;
        gold -= amount;
        OnGoldChanged.Invoke(gold);
        return true;
    }

    public void AddGold(long amount)
    {
        if (amount <= 0) return;
        gold += amount;
        OnGoldChanged.Invoke(gold);
    }

    public void Refund(long amount) => AddGold(amount);

    public string GetGoldDisplay()
    {
        return gold.ToString() + " " + currencyLabel;
    }
}
