using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Game Creation Tool/Quest")]
public class QuestData : ScriptableObject
{
    public enum QuestType
    {
        Principale,
        Secondaire,
        Journalière
    }

    [Header("Identification")]
    public string questId;

    [Header("Informations générales")]
    public QuestType questType;
    public string questName;
    [TextArea(3, 5)]
    public string description;

    [Header("Étapes de la quête")]
    public List<QuestStep> steps = new();

    [Header("Récompenses")]
    public int experienceReward;
    public int coinReward;
    public List<ItemReward> itemRewards = new();
}
