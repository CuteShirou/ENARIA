using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatTabManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform tabsContainer;
    [SerializeField] private Transform panelsContainer;
    [SerializeField] private Button tabPrefab;
    [SerializeField] private GameObject panelPrefab;

    private Dictionary<string, Button> tabButtons = new();
    private Dictionary<string, GameObject> chatPanels = new();
    private string currentTab = "Global";

    private void Awake()
    {
        CreateTab("Global");
        ShowTab("Global");
    }

    public void CreateTab(string tabName)
    {
        if (tabButtons.ContainsKey(tabName)) return;

        var btn = Instantiate(tabPrefab, tabsContainer);
        btn.GetComponentInChildren<Text>().text = tabName;
        btn.onClick.AddListener(() => ShowTab(tabName));
        tabButtons[tabName] = btn;

        var panel = Instantiate(panelPrefab, panelsContainer);
        panel.name = tabName;
        panel.SetActive(false);
        chatPanels[tabName] = panel;
    }

    public void ShowTab(string tabName)
    {
        foreach (var kv in chatPanels)
            kv.Value.SetActive(kv.Key == tabName);
        currentTab = tabName;
    }

    public void AppendMessage(string tabName, string message)
    {
        if (!chatPanels.ContainsKey(tabName))
            CreateTab(tabName);

        var panel = chatPanels[tabName];
        var text = panel.GetComponentInChildren<ScrollRect>().content.GetComponentInChildren<Text>();
        text.text += message + "\n";
        var sb = panel.GetComponentInChildren<Scrollbar>();
        sb.value = 0;
    }

    public string CurrentTab => currentTab;
}
