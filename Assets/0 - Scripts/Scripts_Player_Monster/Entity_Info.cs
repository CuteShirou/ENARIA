using System.Collections.Generic;
using UnityEngine;

public enum SpecieType
{
    Humain,
    Elfe
}

public class Entity_Info : MonoBehaviour
{
    [Header("Information Complèmentaire:")]

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

    [Header("Position Sauvegardé de l'Entité")]
    [SerializeField] public Vector3 savePosEntity;

    [Header("Camera Sauvegardé de l'Entité")]
    [SerializeField] public string saveCamEntity;

    [Header("Type de l'Entité")]
    [SerializeField] public SpecieType specie;

    [Header("Progression (XP / Level)")]
    [SerializeField] public int experience = 0;          // XP accumulée au niveau courant
    [SerializeField] public int remainingPoints = 0;     // Points à répartir gagnés à chaque niveau
    [SerializeField] public int baseXPToLevelUp = 100;   // XP de base pour passer du niveau 1 au 2
    [Range(1f, 3f)] public float xpGrowthFactor = 1.5f;  // Facteur multiplicatif par niveau

    private void Start()
    {
        // Recalcule le niveau et les points à partir de l'XP stockée (utile si on charge une sauvegarde)
        RecalculateLevelFromXP();
    }

    public int GetExperienceToNextLevel()
    {
        // Retourne l'XP nécessaire pour passer au prochain niveau à partir du niveau actuel
        return Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, entity_Level - 1));
    }

    public void GainExperience(int amount)
    {
        // Ajoute de l'XP puis applique autant de montées de niveau que nécessaire
        experience += amount;

        // On boucle tant qu'on a assez d'XP pour monter
        while (experience >= GetExperienceToNextLevel())
        {
            // On retire le coût du niveau courant
            experience -= GetExperienceToNextLevel();
            // On applique la montée de niveau
            LevelUp();
        }
    }

    public void RecalculateLevelFromXP()
    {
        // Recalcule le niveau et l'XP restante à partir d'une XP totale (utile après chargement)
        // Hypothèse : "experience" peut contenir une XP totale historique ; on la convertit
        int tempLevel = 1;
        int tempXP = experience;

        // Soustrait le coût successif de chaque niveau jusqu'à ne plus pouvoir monter
        while (tempXP >= Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, tempLevel - 1)))
        {
            tempXP -= Mathf.RoundToInt(baseXPToLevelUp * Mathf.Pow(xpGrowthFactor, tempLevel - 1));
            tempLevel++;
        }

        // Met à jour le niveau de l'entité et l'XP restante au niveau courant
        entity_Level = tempLevel;
        experience = tempXP;

        // Met à jour les points restants (10 par niveau gagné, hors niveau 1)
        remainingPoints = (entity_Level - 1) * 10;
    }

    private void LevelUp()
    {
        // Incrémente le niveau et crédite les points à répartir
        entity_Level++;
        remainingPoints += 10;
    }
}

[System.Serializable]
public class DropRessource
{
    [Tooltip("Prefab de la ressource à drop")]
    public GameObject ressourcePrefab;

    [Tooltip("Pourcentage de chance de drop (0-100)")]
    [Range(0f, 100f)] public float dropChance;
}
