using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static QuestData;

public class QuestMenuUI : MonoBehaviour
{
    [Header("Quest System")]
    public QuestManager questManager;
    public QuestInfoUI questInfoUI;

    [Header("Principal")]
    public Button mainHeader;
    public Transform mainListContent;

    [Header("Secondaire")]
    public Button secondaryHeader;
    public Transform secondaryListContent;

    [Header("Journalière")]
    public Button dailyHeader;
    public Transform dailyListContent;

    [Header("Item Prefab")]
    public QuestListItem questListItemPrefab;

    private void Awake()
    {
        mainListContent.gameObject.SetActive(false);
        secondaryListContent.gameObject.SetActive(false);
        dailyListContent.gameObject.SetActive(false);

        mainHeader.onClick.AddListener(() => ToggleSection(mainListContent));
        secondaryHeader.onClick.AddListener(() => ToggleSection(secondaryListContent));
        dailyHeader.onClick.AddListener(() => ToggleSection(dailyListContent));
    }

    public void OpenQuestMenu()
    {
        gameObject.SetActive(true);
        PopulateAll();
    }

    private void ToggleSection(Transform content)
    {
        content.gameObject.SetActive(!content.gameObject.activeSelf);
    }

    private void PopulateAll()
    {
        PopulateSection(mainListContent, QuestType.Principale);
        PopulateSection(secondaryListContent, QuestType.Secondaire);
        PopulateSection(dailyListContent, QuestType.Journalière);
    }

    private void PopulateSection(Transform content, QuestType type)
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        List<string> names = questManager.GetQuestNamesByType(type);
        if (names.Count == 0)
        {
            var noneItem = Instantiate(questListItemPrefab, content);
            noneItem.Setup("aucune quête", _ => { });
            noneItem.GetComponent<Button>().interactable = false;
            return;
        }

        foreach (var name in names)
        {
            var item = Instantiate(questListItemPrefab, content);
            item.Setup(name, OnQuestClicked);
        }
    }

    private void OnQuestClicked(string questName)
    {
        var inst = questManager.GetQuestInstanceByNameAndType(questName, QuestData.QuestType.Principale)
                ?? questManager.GetQuestInstanceByNameAndType(questName, QuestData.QuestType.Secondaire)
                ?? questManager.GetQuestInstanceByNameAndType(questName, QuestData.QuestType.Journalière);

        if (inst == null)
        {
            Debug.LogWarning($"Quête introuvable : {questName}");
            return;
        }

        questInfoUI.SetQuestInfo(inst.questData);

        questInfoUI.gameObject.SetActive(true);
    }

}