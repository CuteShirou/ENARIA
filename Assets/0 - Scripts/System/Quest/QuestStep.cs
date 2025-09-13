// QuestStep.cs
using UnityEngine;

[System.Serializable]
public class QuestStep
{
    public string stepName;
    [TextArea(2, 4)]
    public string stepDescription;
    public bool isOptional;

    public QuestObjectiveType objectiveType;

    // Kill
    public int monsterKillCount;

    // Talk
    public string npcName;

    // Collect (utilise Item maintenant)
    public Item itemToCollect;
    public int itemQuantity;

    // Reach
    public string locationName;

    /// <summary>
    /// Retourne la description complète de l'étape (pour tooltip / UI).
    /// currentProgress = valeur actuelle (ex: nombre d'items collectés).
    /// </summary>
    public string GetFullDescription(int currentProgress = 0)
    {
        string optionalText = isOptional ? " (Optionnel)" : "";

        string objectiveText = objectiveType switch
        {
            QuestObjectiveType.TalkToNPC =>
                $"Dialoguer avec \"{npcName}\"",

            QuestObjectiveType.CollectItem =>
                itemToCollect != null
                    ? $"Apporter \"{GetItemName(itemToCollect)}\": {currentProgress}/{itemQuantity}"
                    : $"Apporter des objets: {currentProgress}/{itemQuantity}",

            QuestObjectiveType.ReachLocation =>
                $"Aller à \"{locationName}\"",

            QuestObjectiveType.KillMonster =>
                $"Tuer {monsterKillCount} monstres ({currentProgress}/{monsterKillCount})",

            _ => "Objectif inconnu"
        };

        return
            $"<b>{stepName}</b>{optionalText}\n" +
            $"{stepDescription}\n" +
            $"{objectiveText}";
    }

    private string GetItemName(Item item)
    {
        if (item == null) return "Objet inconnu";
        return string.IsNullOrEmpty(item.itemName) ? "Objet inconnu" : item.itemName;
    }
}
