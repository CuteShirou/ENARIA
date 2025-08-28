using UnityEngine;

[System.Serializable]
public class QuestStep
{
    public string stepName;
    [TextArea(2, 4)]
    public string stepDescription;
    public bool isOptional;

    public QuestObjectiveType objectiveType;

    public int monsterKillCount;

    public string npcName;

    public CollectibleData itemToCollect;
    public int itemQuantity;

    public string locationName;

    public string GetFullDescription(int currentProgress = 0)
    {
        string optionalText = isOptional ? " (Optionnel)" : "";

        string objectiveText = objectiveType switch
        {
            QuestObjectiveType.TalkToNPC =>
                $"Dialogu� avec \"{npcName}\"",

            QuestObjectiveType.CollectItem =>
                itemToCollect != null
                    ? $"Apport� \"{GetCollectibleName(itemToCollect)}\": {currentProgress}/{itemQuantity}"
                    : $"Apport� des objets: {currentProgress}/{itemQuantity}",

            QuestObjectiveType.ReachLocation =>
                $"Aller � \"{locationName}\"",

            _ => "Objectif inconnu"
        };

        return
            $"<b>{stepName}</b>{optionalText}\n" +
            $"{stepDescription}\n" +
            $"{objectiveText}";
    }

    // m�thode priv�e locale pour r�cup�rer le nom exact selon le type
    private string GetCollectibleName(CollectibleData collectible)
    {
        if (collectible == null)
            return "Objet inconnu";

        if (collectible is EquipmentData equip)
            return equip.equipmentName;

        if (collectible is ResourceData res)
            return res.resourceName;

        return "Objet inconnu";
    }

}
