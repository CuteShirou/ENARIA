using UnityEngine;

public class QuestTestButton : MonoBehaviour
{
    public QuestManager questManager;
    public QuestData questToAccept;
    public QuestSaver questSaver;

    public void AcceptQuestButton()
    {
        if (questManager != null && questToAccept != null && questSaver != null)
        {
            questManager.AcceptQuest(questToAccept);

            string playerId = "1";
            string questId = questToAccept.questId;

            questSaver.SaveQuestToServer(
                playerId,
                questId,
                isAccepted: 1,
                isCompleted: 0,
                currentStepIndex: 0,
                stepProgress: 0  
            );

        }
        else
        {
            Debug.LogWarning("Une des références (QuestManager, QuestData, QuestSaver) est manquante !");
        }
    }
}
