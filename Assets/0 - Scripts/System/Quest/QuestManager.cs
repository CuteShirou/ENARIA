// QuestManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static QuestData;

public class QuestManager : MonoBehaviour
{
    [Header("Données")]
    public List<QuestData> allQuests;
    public List<QuestInstance> activeQuests = new();
    public List<QuestInstance> trackedQuests = new();

    [Header("Réseau")]
    public QuestSaver questSaver;
    public int playerId = 1;

    private Dictionary<string, QuestData> _lookup;
    private string _savePath;

    [System.Serializable]
    private class QuestListWrapper
    {
        public List<QuestInstanceDTO> quests;
    }

    private void Awake()
    {
        // build lookup
        _lookup = new Dictionary<string, QuestData>();
        foreach (var q in allQuests)
        {
            if (!_lookup.ContainsKey(q.questId))
                _lookup.Add(q.questId, q);
        }

        // safe compute save path
        if (string.IsNullOrEmpty(_savePath))
            _savePath = Path.Combine(Application.persistentDataPath ?? Application.dataPath, "quests_save.json");

        Debug.Log($"[QuestManager] Awake - savePath = {_savePath}");

        LoadQuests();
    }


    public void AcceptQuest(QuestData data)
    {
        if (activeQuests.Exists(q => q.questData == data))
            return;

        var instance = new QuestInstance
        {
            questData = data,
            isAccepted = true,
            isCompleted = false,
            currentStepIndex = 0,
            stepProgress = new Dictionary<int, int>()
        };
        activeQuests.Add(instance);

        Debug.Log($"[QuestManager] Quête acceptée : {data.questName} ({data.questType})");

        if (questSaver != null)
        {
            questSaver.SaveQuestToServer(
                playerId.ToString(),
                data.questId,
                isAccepted: 1,
                isCompleted: 0,
                currentStepIndex: 0,
                stepProgress: 0
            );
        }
        else
        {
            Debug.LogWarning("QuestSaver non assigné dans QuestManager !");
        }

        SaveQuests();
    }

    public List<string> GetQuestNamesByType(QuestType type)
    {
        List<string> result = new();
        foreach (var quest in activeQuests)
        {
            if (quest.isAccepted && !quest.isCompleted && quest.questData.questType == type)
                result.Add(quest.questData.questName);
        }
        return result;
    }

    public QuestInstance GetQuestInstanceByNameAndType(string name, QuestType type)
    {
        return activeQuests.Find(q =>
            q.questData.questType == type && q.questData.questName == name
        );
    }

    public void SaveQuests()
    {
        try
        {
            if (string.IsNullOrEmpty(_savePath))
            {
                _savePath = Path.Combine(Application.persistentDataPath ?? Application.dataPath, "quests_save.json");
                Debug.LogWarning("[QuestManager] Save path was null/empty, recomputed to: " + _savePath);
            }

            var wrapper = new QuestListWrapper();
            wrapper.quests = activeQuests.ConvertAll(q => q.ToDTO());
            string json = JsonUtility.ToJson(wrapper, prettyPrint: true);

            if (string.IsNullOrEmpty(_savePath))
            {
                Debug.LogError("[QuestManager] SaveQuests: _savePath still null/empty, aborting save.");
                return;
            }

            File.WriteAllText(_savePath, json);
            Debug.Log("Sauvegarde des quêtes effectuée !");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestManager] SaveQuests failed: {ex}");
            // ne rethrow pas : on veut éviter que la sauvegarde plante tout le flow de jeu
        }
    }


    public void LoadQuests()
    {
        try
        {
            if (!File.Exists(_savePath)) return;
            string json = File.ReadAllText(_savePath);
            var wrapper = JsonUtility.FromJson<QuestListWrapper>(json);
            if (wrapper == null || wrapper.quests == null) return;

            activeQuests.Clear();
            foreach (var dto in wrapper.quests)
            {
                var inst = QuestInstance.FromDTO(dto, _lookup);
                if (inst != null) activeQuests.Add(inst);
            }
            Debug.Log("Quêtes rechargées depuis la sauvegarde.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestManager] LoadQuests failed: {ex}");
            // on continue sans arrêter l'application
        }
    }

    public void FollowQuest(QuestInstance quest)
    {
        if (!trackedQuests.Contains(quest))
        {
            trackedQuests.Add(quest);
            Debug.Log($"[QuestManager] FollowQuest: added '{quest.questData.questName}'. trackedCount={trackedQuests.Count}");
        }
    }

    public void UnfollowQuest(QuestInstance quest)
    {
        if (trackedQuests.Contains(quest))
        {
            trackedQuests.Remove(quest);
            Debug.Log($"[QuestManager] Quête retirée du suivi : {quest.questData.questName}");
        }
    }

    /// <summary>
    /// Complète l'étape courante d'une questInstance.
    /// </summary>
    public void CompleteStep(QuestInstance questInstance)
    {
        if (questInstance == null || questInstance.questData == null)
        {
            Debug.LogWarning("[QuestManager] CompleteStep: questInstance ou questData null !");
            return;
        }

        // si la quête est déjà complétée, on ne fait rien
        if (questInstance.isCompleted)
        {
            Debug.LogWarning("[QuestManager] CompleteStep: la quête est déjà complétée -> rien à faire.");
            return;
        }

        var steps = questInstance.questData.steps;
        int idx = questInstance.currentStepIndex;

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[QuestManager] CompleteStep: aucune step disponible pour cette quête.");
            return;
        }

        // si currentStepIndex est hors limites, on considère la quest déjà terminée ou invalide
        if (idx < 0 || idx >= steps.Count)
        {
            Debug.LogWarning($"[QuestManager] CompleteStep: index hors limites. currentStepIndex={idx}, steps.Count={steps.Count}");
            return;
        }

        var step = steps[idx];
        if (step == null)
        {
            Debug.LogWarning("[QuestManager] CompleteStep: step actuelle est null.");
            return;
        }

        // Appel des handlers selon le type
        switch (step.objectiveType)
        {
            case QuestObjectiveType.CollectItem:
                if (step.itemToCollect != null)
                    CollectItem(step.itemToCollect, questInstance, idx);
                else
                    Debug.LogWarning("[QuestManager] CollectItem: itemToCollect est null.");
                break;

            case QuestObjectiveType.KillMonster:
                KillMonster(step.monsterKillCount, questInstance, idx);
                break;

            case QuestObjectiveType.TalkToNPC:
                TalkToNPC(step.npcName, questInstance, idx);
                break;

            case QuestObjectiveType.ReachLocation:
                ReachLocation(step.locationName, questInstance, idx);
                break;

            default:
                Debug.LogWarning("[QuestManager] CompleteStep: type d'objectif non géré.");
                break;
        }

        // assure l'entrée de progress
        if (!questInstance.stepProgress.ContainsKey(idx))
            questInstance.stepProgress[idx] = 0;

        int required = step.objectiveType == QuestObjectiveType.CollectItem ? step.itemQuantity :
                       step.objectiveType == QuestObjectiveType.KillMonster ? step.monsterKillCount : 1;

        int current = questInstance.stepProgress[idx];

        // Si la step est atteinte (ou a été atteinte par le handler), on avance.
        if (current >= required)
        {
            AdvanceStep(questInstance);
        }
        else
        {
            Debug.LogWarning($"[QuestManager] CompleteStep: step pas encore atteinte ({current}/{required}).");
        }

        // sauvegarde protégée
        SaveQuests();
    }



    // --- Handlers simples (stubs) ---

    public void CollectItem(Item item, QuestInstance questInstance = null, int stepIndex = -1)
    {
        Debug.Log($"[QuestManager] CollectItem called for '{item?.itemName ?? "NULL"}'");

        if (questInstance != null && stepIndex >= 0)
        {
            int cur = questInstance.stepProgress.ContainsKey(stepIndex) ? questInstance.stepProgress[stepIndex] : 0;
            cur++;
            questInstance.stepProgress[stepIndex] = cur;

            var step = questInstance.questData.steps[stepIndex];
            if (cur >= step.itemQuantity)
            {
                Debug.Log($"[QuestManager] Objectif collecte atteint pour la step {stepIndex}.");
            }
        }
    }

    private void AdvanceStep(QuestInstance qi)
    {
        int idx = qi.currentStepIndex;
        var steps = qi.questData.steps;
        if (steps == null || idx < 0 || idx >= steps.Count) return;

        if (!qi.stepProgress.ContainsKey(idx))
        {
            var s = steps[idx];
            int required = (s.objectiveType == QuestObjectiveType.CollectItem) ? s.itemQuantity :
                           (s.objectiveType == QuestObjectiveType.KillMonster) ? s.monsterKillCount : 1;
            qi.stepProgress[idx] = required;
        }

        qi.currentStepIndex++;
        Debug.Log($"[QuestManager] AdvanceStep: quest='{qi.questData.questName}' newStepIndex={qi.currentStepIndex}");

        if (qi.currentStepIndex >= (steps?.Count ?? 0))
        {
            qi.isCompleted = true;
            Debug.Log($"[QuestManager] Quête complétée : {qi.questData.questName}");
        }

        SaveQuests();
    }

    public void KillMonster(int count, QuestInstance questInstance = null, int stepIndex = -1)
    {
        Debug.Log($"[QuestManager] KillMonster called: target={count}");
        if (questInstance != null && stepIndex >= 0)
        {
            int cur = questInstance.stepProgress.ContainsKey(stepIndex) ? questInstance.stepProgress[stepIndex] : 0;
            cur++;
            questInstance.stepProgress[stepIndex] = cur;

            var step = questInstance.questData.steps[stepIndex];
            if (cur >= step.monsterKillCount)
            {
                Debug.Log($"[QuestManager] Objectif kill atteint pour la step {stepIndex}.");
            }
        }
    }

    public void TalkToNPC(string npcName, QuestInstance questInstance = null, int stepIndex = -1)
    {
        Debug.Log($"[QuestManager] TalkToNPC called: {npcName}");
    }

    public void ReachLocation(string locationName, QuestInstance questInstance = null, int stepIndex = -1)
    {
        Debug.Log($"[QuestManager] ReachLocation called: {locationName}");
    }

    public void RegisterCollectedItem(Item item)
    {
        if (item == null) return;
        for (int i = 0; i < activeQuests.Count; i++)
        {
            var qi = activeQuests[i];
            if (!qi.isAccepted || qi.isCompleted) continue;

            int idx = qi.currentStepIndex;
            if (idx < 0 || idx >= (qi.questData.steps?.Count ?? 0)) continue;
            var step = qi.questData.steps[idx];

            if (step.objectiveType == QuestObjectiveType.CollectItem && step.itemToCollect != null
                && step.itemToCollect.id == item.id)
            {
                int cur = qi.stepProgress.ContainsKey(idx) ? qi.stepProgress[idx] : 0;
                cur++;
                qi.stepProgress[idx] = cur;
                Debug.Log($"[QuestManager] RegisterCollectedItem: quest='{qi.questData.questName}' step={idx} progress={cur}/{step.itemQuantity}");

                if (cur >= step.itemQuantity)
                    AdvanceStep(qi);
            }
        }
        SaveQuests();
    }

    public void RegisterKill(string monsterIdOrName)
    {
        if (string.IsNullOrEmpty(monsterIdOrName)) return;
        for (int i = 0; i < activeQuests.Count; i++)
        {
            var qi = activeQuests[i];
            if (!qi.isAccepted || qi.isCompleted) continue;

            int idx = qi.currentStepIndex;
            if (idx < 0 || idx >= (qi.questData.steps?.Count ?? 0)) continue;
            var step = qi.questData.steps[idx];

            if (step.objectiveType == QuestObjectiveType.KillMonster)
            {
                int cur = qi.stepProgress.ContainsKey(idx) ? qi.stepProgress[idx] : 0;
                cur++;
                qi.stepProgress[idx] = cur;
                Debug.Log($"[QuestManager] RegisterKill: quest='{qi.questData.questName}' step={idx} progress={cur}/{step.monsterKillCount}");

                if (cur >= step.monsterKillCount)
                    AdvanceStep(qi);
            }
        }
        SaveQuests();
    }

    public void RegisterTalkToNPC(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return;
        for (int i = 0; i < activeQuests.Count; i++)
        {
            var qi = activeQuests[i];
            if (!qi.isAccepted || qi.isCompleted) continue;

            int idx = qi.currentStepIndex;
            if (idx < 0 || idx >= (qi.questData.steps?.Count ?? 0)) continue;
            var step = qi.questData.steps[idx];

            if (step.objectiveType == QuestObjectiveType.TalkToNPC && step.npcName == npcName)
            {
                AdvanceStep(qi);
            }
        }
        SaveQuests();
    }

    public void RegisterReachLocation(string locationName)
    {
        if (string.IsNullOrEmpty(locationName)) return;
        for (int i = 0; i < activeQuests.Count; i++)
        {
            var qi = activeQuests[i];
            if (!qi.isAccepted || qi.isCompleted) continue;

            int idx = qi.currentStepIndex;
            if (idx < 0 || idx >= (qi.questData.steps?.Count ?? 0)) continue;
            var step = qi.questData.steps[idx];

            if (step.objectiveType == QuestObjectiveType.ReachLocation && step.locationName == locationName)
            {
                AdvanceStep(qi);
            }
        }
        SaveQuests();
    }
}
