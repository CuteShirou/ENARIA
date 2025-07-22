using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestInfoUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questNameText;
    public TMP_Text descriptionText;

    public Transform stepsContainer;
    public GameObject stepPrefab;

    public TMP_Text xpRewardText;
    public TMP_Text coinRewardText;
    public Transform itemRewardsContainer;
    public GameObject itemRewardPrefab;

    public void SetQuestInfo(QuestData quest)
    {
        Debug.Log($"[QuestInfoUI] Affichage de la quête : {(quest != null ? quest.questName : "NULL")}");

        foreach (Transform child in stepsContainer)
            Destroy(child.gameObject);

        if (quest == null)
        {
            ClearUI();
            return;
        }

        questNameText.text = quest.questName;
        descriptionText.text = quest.description;


        Debug.Log($"[QuestInfoUI] Nombre d’étapes : {quest.steps.Count}");
        for (int i = 0; i < quest.steps.Count; i++)
        {
            var step = quest.steps[i];
            Debug.Log($"[QuestInfoUI] Step[{i}] name=\"{step.stepName}\" desc=\"{step.stepDescription}\" " +
                      $"type={step.objectiveType} monsterCount={step.monsterKillCount} npc=\"{step.npcName}\" " +
                      $"itemQty={step.itemQuantity} location=\"{step.locationName}\"");

            var go = Instantiate(stepPrefab, stepsContainer);
            var stepText = go.GetComponentInChildren<TMP_Text>();
            if (stepText != null)
                stepText.text = step.stepName;

            var tooltipTrigger = go.GetComponent<StepTooltipTrigger>();
            if (tooltipTrigger != null)
            {
                tooltipTrigger.step = step;
            }
        }

        xpRewardText.text = $"XP : {quest.experienceReward}";
        coinRewardText.text = $"Pièces : {quest.coinReward}";

        foreach (Transform child in itemRewardsContainer)
            Destroy(child.gameObject);

        foreach (var item in quest.itemRewards)
        {
            var go = Instantiate(itemRewardPrefab, itemRewardsContainer);
            var text = go.GetComponentInChildren<TMP_Text>();

            if (text != null && item.collectible != null)
            {
                string name = GetCollectibleName(item.collectible);
                text.text = $"{name} x{item.quantity}";
            }
        }
    }

    void ClearUI()
    {
        questNameText.text = "";
        descriptionText.text = "";
        xpRewardText.text = "";
        coinRewardText.text = "";

        foreach (Transform child in stepsContainer)
            Destroy(child.gameObject);
        foreach (Transform child in itemRewardsContainer)
            Destroy(child.gameObject);
    }

    private string GetCollectibleName(CollectibleData collectible)
    {
        if (collectible == null)
            return "Objet inconnu";

        if (collectible is EquipmentData equip)
            return equip.equipmentName;

        if (collectible is ResourceData res)
            return res.resourceName;

        return "Objet inconnu";
    }
}
