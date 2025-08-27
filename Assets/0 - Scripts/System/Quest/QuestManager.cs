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

    private void Awake()
    {
        _lookup = new Dictionary<string, QuestData>();
        foreach (var q in allQuests)
        {
            if (!_lookup.ContainsKey(q.questId))
                _lookup.Add(q.questId, q);
        }

        _savePath = Path.Combine(Application.persistentDataPath, "quests_save.json");
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
        Debug.Log($"[QuestManager] Filtrage pour type {type}");
        foreach (var quest in activeQuests)
        {
            Debug.Log($"  {quest.questData.questName}, type={quest.questData.questType}, accepted={quest.isAccepted}, completed={quest.isCompleted}");
            if (quest.isAccepted && !quest.isCompleted && quest.questData.questType == type)
                result.Add(quest.questData.questName);
        }
        Debug.Log($"[QuestManager] Résultat pour {type} : {string.Join(", ", result)}");
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
        var wrapper = new QuestListWrapper();
        wrapper.quests = activeQuests.ConvertAll(q => q.ToDTO());
        string json = JsonUtility.ToJson(wrapper, prettyPrint: true);
        File.WriteAllText(_savePath, json);
        Debug.Log("Sauvegarde des quêtes effectuée !");
    }

    public void LoadQuests()
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

    public void FollowQuest(QuestInstance quest)
    {
        if (!trackedQuests.Contains(quest))
        {
            trackedQuests.Add(quest);
            Debug.Log($"[QuestManager] FollowQuest: added '{quest.questData.questName}'. trackedCount={trackedQuests.Count}");
        }
        else
        {
            Debug.Log($"[QuestManager] FollowQuest: already tracked '{quest.questData.questName}'. trackedCount={trackedQuests.Count}");
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

    [System.Serializable]
    private class QuestListWrapper
    {
        public List<QuestInstanceDTO> quests;
    }
}
