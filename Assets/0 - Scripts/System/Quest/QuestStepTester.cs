// QuestDataTest.cs (version corrigée pour CollectItem)
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class QuestDataTest : MonoBehaviour
{
    public QuestManager questManager;   // drag QuestManager
    public QuestData questData;         // drag QuestData asset
    public Button testButton;

    [Header("Player pour récompenses")]
    public Entity_Info player;          // <-- glisser-déposer le GameObject player ici

    [Header("GO à activer quand la quête est complétée")]
    public GameObject[] goToActivate;

    [Header("GO à désactiver quand la quête est complétée")]
    public GameObject[] goToDeactivate;

    private const int REWARD_MARK_KEY = -1;

    private void Start()
    {
        if (testButton != null)
            testButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (questManager == null || questData == null || player == null)
        {
            Debug.LogWarning("QuestManager, QuestData ou Player non assigné !");
            return;
        }

        // Récupère / crée l'instance de quête
        var questInstance = questManager.activeQuests.Find(q => q.questData == questData);
        if (questInstance == null)
        {
            questManager.AcceptQuest(questData);
            questInstance = questManager.activeQuests.Find(q => q.questData == questData);
            Debug.Log("Quest accepted via tester: " + questData.questName);
        }

        if (questInstance == null)
        {
            Debug.LogError("Impossible de récupérer l'instance de la quête.");
            return;
        }

        // Vérification étape actuelle
        int idx = questInstance.currentStepIndex;
        if (questInstance.questData.steps == null || idx < 0 || idx >= questInstance.questData.steps.Count)
        {
            Debug.LogWarning("Aucune step valide pour cette quête.");
            return;
        }

        var step = questInstance.questData.steps[idx];

        // --- Traitement spécifique pour CollectItem ---
        if (step.objectiveType == QuestObjectiveType.CollectItem)
        {
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

            // Consommation réelle : on supprime item(s) et on notifie questManager via RegisterCollectedItem
            int remaining = need;
            int slots = InventoryManager.Instance.SlotCapacity;
            for (int i = 0; i < slots && remaining > 0; i++)
            {
                var it = InventoryManager.Instance.GetItemAt(i);
                if (it == null || it.id != neededItem.id) continue;

                int available = InventoryManager.Instance.GetCountAt(i);
                int toRemove = Mathf.Min(available, remaining);
                if (toRemove <= 0) continue;

                if (InventoryManager.Instance.RemoveAmountAt(i, toRemove))
                {
                    for (int r = 0; r < toRemove; r++)
                        questManager.RegisterCollectedItem(neededItem); // on notifie autant de fois que d'items retirés

                    remaining -= toRemove;
                }
                else
                {
                    Debug.LogWarning("Impossible de retirer " + toRemove + " de la slot " + i);
                }
            }

            if (remaining > 0)
            {
                Debug.Log("Pas assez d'items dans l'inventaire : manquent " + remaining + "/" + need + " '" + neededItem.itemName + "'.");
                // IMPORTANT : NE PAS appeler CompleteStep ici — l'objectif n'est pas rempli
            }
            else
            {
                Debug.Log("Item(s) consommé(s) pour la quête '" + questData.questName + "' : " + need + "/" + need + " '" + neededItem.itemName + "'.");
                // Là, RegisterCollectedItem a déjà été appelé et a (si nécessaire) appelé AdvanceStep
                // donc on NE DOIT PAS appeler CompleteStep après cela (sinon double comptage).
            }
        }
        else
        {
            // Pour tout autre type d'objectif, appeler CompleteStep est OK
            if (!questInstance.isCompleted && questInstance.currentStepIndex < questInstance.questData.steps.Count)
                questManager.CompleteStep(questInstance);
        }

        // --- Attribution des récompenses XP & gold (seul si la quête est complétée) ---
        bool alreadyRewarded = questInstance.stepProgress != null && questInstance.stepProgress.ContainsKey(REWARD_MARK_KEY);
        if (questInstance.isCompleted && !alreadyRewarded)
        {
            GiveRewards(questInstance);

            if (questInstance.stepProgress == null)
                questInstance.stepProgress = new System.Collections.Generic.Dictionary<int, int>();
            questInstance.stepProgress[REWARD_MARK_KEY] = 1;

            // --- Activation des GO ---
            if (goToActivate != null)
            {
                foreach (var go in goToActivate)
                {
                    if (go != null)
                        go.SetActive(true);
                }
            }

            // --- Désactivation des GO ---
            if (goToDeactivate != null)
            {
                foreach (var go in goToDeactivate)
                {
                    if (go != null)
                        go.SetActive(false);
                }
            }

            try { questManager.SaveQuests(); } catch { }

            Debug.Log("Récompenses attribuées pour la quête: " + questData.questName);
        }

        Debug.Log("Test terminé. stepIndex = " + questInstance.currentStepIndex);
    }

    private void GiveRewards(QuestInstance qi)
    {
        var qd = qi.questData;
        if (qd == null || player == null) return;

        // XP
        if (qd.experienceReward > 0)
        {
            player.GainExperience(qd.experienceReward);
            Debug.LogWarning($"XP ajoutée : {qd.experienceReward} -> total player XP maintenant = {player.experience}");
        }

        // Gold
        if (qd.coinReward > 0)
        {
            player.AddGold(qd.coinReward);
            Debug.LogWarning($"Gold ajouté : {qd.coinReward} -> total player gold maintenant = {player.gold}");
        }
    }
}
