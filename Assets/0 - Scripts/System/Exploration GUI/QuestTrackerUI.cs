using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using static QuestData;

public class QuestTrackerUI : MonoBehaviour
{
    public Transform trackedQuestsContainer;
    public GameObject trackedQuestPrefab;
    public QuestManager questManager;

    private List<QuestInstance> trackedQuests = new List<QuestInstance>();

    public void SetTrackedQuests(List<QuestInstance> quests)
    {
        Debug.Log($"[QuestTrackerUI] SetTrackedQuests called. quests==null? {quests == null} count={(quests == null ? 0 : quests.Count)}");

        trackedQuests = quests;

        foreach (Transform child in trackedQuestsContainer)
            Destroy(child.gameObject);

        if (trackedQuests == null || trackedQuests.Count == 0)
            return;

        if (trackedQuestPrefab == null)
        {
            Debug.LogError("[QuestTrackerUI] trackedQuestPrefab is null!");
            return;
        }
        if (trackedQuestsContainer == null)
        {
            Debug.LogError("[QuestTrackerUI] trackedQuestsContainer is null!");
            return;
        }

        foreach (var questInstance in trackedQuests)
        {
            var go = Instantiate(trackedQuestPrefab, trackedQuestsContainer);
            if (go == null)
            {
                Debug.LogError("[QuestTrackerUI] Instantiate returned null for quest " + questInstance.questData.questName);
                continue;
            }

            go.SetActive(true);
            if (!go.activeInHierarchy)
                Debug.LogWarning("[QuestTrackerUI] instantiated go is not active in hierarchy: " + go.name);

            var button = go.GetComponent<TrackedQuestButton>();
            if (button != null)
            {
                button.Setup(questInstance);
                Debug.Log("[QuestTrackerUI] Setup called for: " + questInstance.questData.questName);
            }
            else
            {
                Debug.LogWarning("[QuestTrackerUI] TrackedQuestButton missing on prefab: " + go.name);
                var tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmp != null)
                {
                    tmp.enabled = true;
                    tmp.text = questInstance.questData.questName + " (no TrackedQuestButton)";
                }
            }

            var childText = go.GetComponentInChildren<TMPro.TMP_Text>();
            if (childText == null)
                Debug.LogWarning("[QuestTrackerUI] No TMP_Text found in prefab children for " + go.name);
            else
                Debug.Log($"[QuestTrackerUI] TMP found: enabled={childText.enabled}, text='{childText.text}'");

            // --- TOOLTIP: on hover show tooltip using the existing TooltipUI singleton ---
            string tooltipContent = BuildTooltipFor(questInstance);
            AttachTooltipToQuestName(go, tooltipContent);

        }

        Canvas.ForceUpdateCanvases();
    }

    private string BuildTooltipFor(QuestInstance instance)
    {
        var q = instance.questData;
        if (q == null) return "";

        string firstLine = q.description ?? "";
        int newline = firstLine.IndexOf('\n');
        if (newline > 0) firstLine = firstLine.Substring(0, newline);

        return $"<b>{q.questName}</b>\n{firstLine}";
    }

    // utilise le TMP du nom de la quête comme cible pour le tooltip de la quête
    private void AttachTooltipToQuestName(GameObject go, string content)
    {
        if (TooltipUI.Instance == null)
        {
            Debug.LogWarning("[QuestTrackerUI] TooltipUI.Instance is null — tooltip will not show.");
            return;
        }

        // cherche un enfant nommé "QuestNameText" (ou le premier TMP qui a le texte similaire)
        Transform nameTf = go.transform.Find("QuestNameText");
        TMP_Text nameTmp = null;

        if (nameTf != null)
            nameTmp = nameTf.GetComponent<TMP_Text>();
        else
        {
            // fallback : premier TMP enfant dont le texte correspond au nom de la quête
            var tmps = go.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            if (tmps != null && tmps.Length > 0)
                nameTmp = tmps[0];
        }

        if (nameTmp == null)
        {
            Debug.LogWarning("[QuestTrackerUI] QuestNameText not found in prefab children for " + go.name);
            return;
        }

        GameObject target = nameTmp.gameObject;
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        // remove duplicates
        trigger.triggers ??= new List<EventTrigger.Entry>();
        RemoveExistingEntries(trigger, EventTriggerType.PointerEnter);
        RemoveExistingEntries(trigger, EventTriggerType.PointerExit);

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback = new EventTrigger.TriggerEvent();
        entryEnter.callback.AddListener((data) => TooltipUI.Instance.Show(content));

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback = new EventTrigger.TriggerEvent();
        entryExit.callback.AddListener((data) => TooltipUI.Instance.Hide());

        trigger.triggers.Add(entryEnter);
        trigger.triggers.Add(entryExit);
    }


    private void RemoveExistingEntries(EventTrigger trigger, EventTriggerType type)
    {
        if (trigger == null || trigger.triggers == null) return;
        trigger.triggers.RemoveAll(e => e.eventID == type);
    }
}
