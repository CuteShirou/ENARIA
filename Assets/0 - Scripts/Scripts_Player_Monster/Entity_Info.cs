using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SpecieType
{
    Humain,
    Elfe,
    Monster
}

public class Entity_Info : MonoBehaviour
{
    [Header("Information Complémentaire:")]

    [Header("Pseudo de l'Entité")]
    [SerializeField] public string entity_Name;

    [Header("Level de l'Entité")]
    [SerializeField] public int entity_Level;

    [Header("Icon (Sprite) de l'Entité")]
    [SerializeField] public Sprite entity_Icon;

    [Header("Liste des Ressources Dropables sur lui")]
    public List<DropRessource> listDropRessources = new();

    [Header("Gain d'Xp si vaincu")]
    [SerializeField] public float gainXp;

    [Header("Position Sauvegardée de l'Entité")]
    [SerializeField] public Vector3 savePosEntity;

    [Header("Camera Sauvegardée de l'Entité")]
    [SerializeField] public string saveCamEntity;

    [Header("Type de l'Entité")]
    [SerializeField] public SpecieType specie;

    [Header("Progression (XP / Level)")]
    [SerializeField] public int experience = 0;          // XP accumulée au niveau courant
    [SerializeField] public int remainingPoints = 0;     // Points à répartir gagnés à chaque niveau
    [SerializeField] public int baseXPToLevelUp = 100;   // XP de base pour passer du niveau 1 au 2
    [Range(1f, 3f)] public float xpGrowthFactor = 1.5f;  // Facteur multiplicatif par niveau

    [Header("Monnaie")]
    [Tooltip("Montant d'argent courant de l'entité.")]
    public long gold;
    [Tooltip("Nom de la devise (affichée dans l'UI).")]
    public string currencyLabel = "";
    public UnityEvent<long> OnGoldChanged;

    private void Start()
    {
        // Recalcule le niveau et les points à partir de l'XP stockée
        RecalculateLevelFromXP();

        // Initialise l'événement de monnaie
        if (OnGoldChanged == null)
            OnGoldChanged = new UnityEvent<long>();

        OnGoldChanged.Invoke(gold);
    }

    // --- Gestion de l'expérience ---
    public int GetExperienceToNextLevel()
    {
        return Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, entity_Level - 1));
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        while (experience >= GetExperienceToNextLevel())
        {
            experience -= GetExperienceToNextLevel();
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

        entity_Level = tempLevel;
        experience = tempXP;
        remainingPoints = (entity_Level - 1) * 10;
    }

    private void LevelUp()
    {
        entity_Level++;
        remainingPoints += 10;
    }

    // --- Gestion de l'argent ---
    public bool CanAfford(long amount) => amount <= gold;

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

    public string GetGoldDisplay() => gold.ToString() + " " + currencyLabel;
}

[System.Serializable]
public class DropRessource
{
    [Tooltip("Prefab de la ressource à drop")]
    public GameObject ressourcePrefab;

    [Tooltip("Pourcentage de chance de drop (0-100)")]
    [Range(0f, 100f)] public float dropChance;
}
