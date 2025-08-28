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

    public string questId;

    [Header("General informations")]
    public QuestType questType;
    public string questName;
    [TextArea(3, 5)]
    public string description;

    [Header("Quest steps")]
    public List<QuestStep> steps = new();

    [Header("Rewards")]
    public int experienceReward;
    public int coinReward;
    public List<ItemReward> itemRewards = new();
}
