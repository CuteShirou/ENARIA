using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestInstance
{
    public QuestData questData;

    public bool isAccepted = false;
    public bool isCompleted = false;
    public int currentStepIndex = 0;

    public Dictionary<int, int> stepProgress = new();

    public QuestInstanceDTO ToDTO()
    {
        return new QuestInstanceDTO
        {
            questId = questData.questId,
            isAccepted = isAccepted,
            isCompleted = isCompleted,
            currentStepIndex = currentStepIndex,
            stepProgress = new Dictionary<int, int>(stepProgress)
        };
    }

    public static QuestInstance FromDTO(QuestInstanceDTO dto, Dictionary<string, QuestData> questLookup)
    {
        if (!questLookup.TryGetValue(dto.questId, out QuestData questData))
        {
            Debug.LogWarning($"QuestData introuvable pour questId {dto.questId}");
            return null;
        }

        return new QuestInstance
        {
            questData = questData,
            isAccepted = dto.isAccepted,
            isCompleted = dto.isCompleted,
            currentStepIndex = dto.currentStepIndex,
            stepProgress = dto.stepProgress != null
            ? new Dictionary<int, int>(dto.stepProgress)
            : new Dictionary<int, int>()
        };
    }
}

[System.Serializable]
public class QuestInstanceDTO
{
    public string questId;
    public bool isAccepted;
    public bool isCompleted;
    public int currentStepIndex;
    public Dictionary<int, int> stepProgress;
}
