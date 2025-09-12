// QuestInstance.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestInstance
{
    public QuestData questData;

    public bool isAccepted = false;
    public bool isCompleted = false;
    public int currentStepIndex = 0;

    // progress par step (index -> valeur)
    public Dictionary<int, int> stepProgress = new();

    public QuestInstanceDTO ToDTO()
    {
        var dto = new QuestInstanceDTO
        {
            questId = questData != null ? questData.questId : string.Empty,
            isAccepted = isAccepted,
            isCompleted = isCompleted,
            currentStepIndex = currentStepIndex,
            stepProgressList = new List<IntPair>()
        };

        if (stepProgress != null)
        {
            foreach (var kv in stepProgress)
                dto.stepProgressList.Add(new IntPair { key = kv.Key, value = kv.Value });
        }

        return dto;
    }

    public static QuestInstance FromDTO(QuestInstanceDTO dto, Dictionary<string, QuestData> questLookup)
    {
        if (dto == null)
            return null;

        if (!questLookup.TryGetValue(dto.questId, out QuestData questData))
        {
            Debug.LogWarning($"QuestData introuvable pour questId {dto.questId}");
            return null;
        }

        var inst = new QuestInstance
        {
            questData = questData,
            isAccepted = dto.isAccepted,
            isCompleted = dto.isCompleted,
            currentStepIndex = dto.currentStepIndex,
            stepProgress = new Dictionary<int, int>()
        };

        if (dto.stepProgressList != null)
        {
            foreach (var p in dto.stepProgressList)
                inst.stepProgress[p.key] = p.value;
        }

        return inst;
    }
}
