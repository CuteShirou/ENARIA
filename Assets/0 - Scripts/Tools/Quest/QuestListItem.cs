using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestListItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    private Button button;
    private string questName;
    private Action<string> onClickCallback;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(string name, Action<string> onClick)
    {
        questName = name;
        questNameText.text = name;
        onClickCallback = onClick;
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(questName);
    }
}
