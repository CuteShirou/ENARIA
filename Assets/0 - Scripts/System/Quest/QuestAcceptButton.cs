using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class QuestAcceptButton : MonoBehaviour
{
    [Header("Références")]
    public QuestManager questManager;    // drag & drop QuestManager de la scène
    public QuestData questToAccept;      // drag & drop l'asset QuestData
    public Button acceptButton;          // bouton UI

    private void Start()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);
        else
            Debug.LogWarning("[QuestAcceptButton] Aucun bouton assigné !");
    }

    private void OnAcceptClicked()
    {
        if (questManager == null || questToAccept == null)
        {
            Debug.LogWarning("[QuestAcceptButton] QuestManager ou QuestData non assigné !");
            return;
        }

        // 1) Cherche une instance existante dans activeQuests (par référence ou par questId)
        var existing = questManager.activeQuests
            .FirstOrDefault(qi => qi.questData == questToAccept ||
                                  (qi.questData != null && qi.questData.questId == questToAccept.questId));

        if (existing != null)
        {
            // Si elle existe déjà mais n'est pas acceptée, l'activer proprement
            if (!existing.isAccepted)
            {
                existing.isAccepted = true;
                existing.isCompleted = false;
                if (existing.stepProgress == null) existing.stepProgress = new System.Collections.Generic.Dictionary<int, int>();
                existing.currentStepIndex = Mathf.Clamp(existing.currentStepIndex, 0, (existing.questData.steps?.Count ?? 1) - 1);

                Debug.Log($"[QuestAcceptButton] Instance existante mise à jour et acceptée : {questToAccept.questName}");
                // Force sauvegarde
                try { questManager.SaveQuests(); } catch { }
            }
            else
            {
                Debug.Log($"[QuestAcceptButton] La quête '{questToAccept.questName}' est déjà acceptée.");
            }
        }
        else
        {
            // Si pas d'instance, utilise la méthode du QuestManager (créera une nouvelle QuestInstance)
            questManager.AcceptQuest(questToAccept);
            Debug.Log($"[QuestAcceptButton] AcceptQuest appelé pour '{questToAccept.questName}'");
        }

        // Optionnel : désactive le bouton pour éviter le spam
        if (acceptButton != null)
            acceptButton.interactable = false;
    }
}
