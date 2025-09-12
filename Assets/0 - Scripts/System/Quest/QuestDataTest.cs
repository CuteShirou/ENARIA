using UnityEngine;
using UnityEngine.UI;

public class QuestDataTester : MonoBehaviour
{
    public QuestManager questManager;   // drag QuestManager
    public QuestData questData;         // drag QuestData asset
    public Button testButton;

    private void Start()
    {
        if (testButton != null)
            testButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (questManager == null)
        {
            Debug.LogWarning("QuestManager non assigne !");
            return;
        }

        if (questData == null)
        {
            Debug.LogWarning("QuestData non assigne !");
            return;
        }

        // Ensure quest is accepted
        var questInstance = questManager.activeQuests.Find(q => q.questData == questData);
        if (questInstance == null)
        {
            questManager.AcceptQuest(questData);
            questInstance = questManager.activeQuests.Find(q => q.questData == questData);
            Debug.Log("Quest accepted via tester: " + questData.questName);
        }

        if (questInstance == null)
        {
            Debug.LogError("Impossible de recuperer l'instance de la quete.");
            return;
        }

        // get current step and required item
        int idx = questInstance.currentStepIndex;
        if (questInstance.questData.steps == null || idx < 0 || idx >= questInstance.questData.steps.Count)
        {
            Debug.LogWarning("Aucune step valide pour cette quete.");
            return;
        }

        var step = questInstance.questData.steps[idx];
        if (step.objectiveType != QuestObjectiveType.CollectItem)
        {
            Debug.LogWarning("Step actuelle n'est pas de type CollectItem.");
            return;
        }

        // step.itemToCollect must be an Item (scriptable) and step.itemQuantity the amount required
        var neededItem = step.itemToCollect;
        int need = Mathf.Max(1, step.itemQuantity);

        if (neededItem == null)
        {
            Debug.LogWarning("La step ne contient pas d'itemToCollect.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager introuvable !");
            return;
        }

        // Try to consume needed quantity from inventory
        int remaining = need;
        int slots = InventoryManager.Instance.SlotCapacity;

        for (int i = 0; i < slots && remaining > 0; i++)
        {
            var it = InventoryManager.Instance.GetItemAt(i);
            if (it == null) continue;

            // compare par id (plus fiable si les instances sont canonicalisées)
            if (it.id != neededItem.id) continue;

            // how many we can remove from this slot
            int available = InventoryManager.Instance.GetCountAt(i);
            int toRemove = Mathf.Min(available, remaining);

            if (toRemove <= 0) continue;

            // remove amount from inventory
            bool removed = InventoryManager.Instance.RemoveAmountAt(i, toRemove);
            if (!removed)
            {
                Debug.LogWarning($"Failed to remove {toRemove} from slot {i}");
                continue;
            }

            // notify quest manager once per removed unit (RegisterCollectedItem increments by 1)
            for (int r = 0; r < toRemove; r++)
            {
                questManager.RegisterCollectedItem(neededItem);
            }

            remaining -= toRemove;
        }

        if (remaining > 0)
        {
            Debug.Log($"Pas assez d'items dans l'inventaire : manquent {remaining}/{need} '{neededItem.itemName}'.");
        }
        else
        {
            Debug.Log($"Item(s) consommé(s) pour la quete '{questData.questName}' : {need}/{need} '{neededItem.itemName}'.");
        }

        // Optionnel : si tu veux forcer une re-check et avancer si quelque chose a changé
        questManager.CompleteStep(questInstance);

        Debug.Log("Test terminé. stepIndex now = " + questInstance.currentStepIndex);
    }
}
