using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TrackedQuestButton : MonoBehaviour
{
    public TMP_Text questNameText;
    public Transform stepsContainer;
    public GameObject stepPrefab;

    private bool isExpanded = false;
    private QuestInstance questInstance;

    public void Setup(QuestInstance instance)
    {
        questInstance = instance;
        if (questNameText != null)
            questNameText.text = instance.questData.questName;

        // Par défaut on peut garder visible ou non les steps (ici visible)
        stepsContainer.gameObject.SetActive(true);

        foreach (Transform child in stepsContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < instance.questData.steps.Count; i++)
        {
            var step = instance.questData.steps[i];
            var go = Instantiate(stepPrefab, stepsContainer);
            if (go == null) continue;

            go.SetActive(true);

            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                int currentProgress = instance.stepProgress.ContainsKey(i) ? instance.stepProgress[i] : 0;
                string progressText = step.objectiveType switch
                {
                    QuestObjectiveType.KillMonster => $"{currentProgress}/{step.monsterKillCount}",
                    QuestObjectiveType.CollectItem => $"{currentProgress}/{step.itemQuantity}",
                    _ => ""
                };

                bool isStepCompleted = i < instance.currentStepIndex ||
                                       (i == instance.currentStepIndex && currentProgress >=
                                        (step.objectiveType == QuestObjectiveType.KillMonster ? step.monsterKillCount :
                                         step.objectiveType == QuestObjectiveType.CollectItem ? step.itemQuantity : 1));

                text.text = $"{step.stepName} {(isStepCompleted ? "Completé" : progressText)}";
                text.color = isStepCompleted ? Color.green : Color.black;
            }

            // --- Tooltip per-step: show the step's full description with progress ---
            if (TooltipUI.Instance != null)
            {
                int currentProgressCapture = instance.stepProgress.ContainsKey(i) ? instance.stepProgress[i] : 0;
                string stepTooltip = step.GetFullDescription(currentProgressCapture);

                // prefer to attach trigger to the TMP text object (if exists) so events are handled on the visible element
                GameObject targetForTrigger = null;
                if (text != null)
                    targetForTrigger = text.gameObject;
                else
                    targetForTrigger = go;

                EventTrigger trigger = targetForTrigger.GetComponent<EventTrigger>();
                if (trigger == null) trigger = targetForTrigger.AddComponent<EventTrigger>();

                // clear existing entries to avoid duplicates
                if (trigger.triggers != null) trigger.triggers.Clear();
                else trigger.triggers = new List<EventTrigger.Entry>();

                // PointerEnter -> Show
                var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                entryEnter.callback = new EventTrigger.TriggerEvent();
                entryEnter.callback.AddListener((data) => TooltipUI.Instance.Show(stepTooltip));

                // PointerExit -> Hide
                var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                entryExit.callback = new EventTrigger.TriggerEvent();
                entryExit.callback.AddListener((data) => TooltipUI.Instance.Hide());

                trigger.triggers.Add(entryEnter);
                trigger.triggers.Add(entryExit);
            }

        }
    }

    public void OnButtonClick()
    {
        isExpanded = !isExpanded;
        stepsContainer.gameObject.SetActive(isExpanded);
        if (isExpanded)
            LayoutRebuilder.ForceRebuildLayoutImmediate(stepsContainer as RectTransform);
    }
}
