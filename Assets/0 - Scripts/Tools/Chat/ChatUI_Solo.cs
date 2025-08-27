using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI_Solo : MonoBehaviour
{
    [Header("Options de démarrage")]
    [Tooltip("Si coché, le chat est ouvert automatiquement au lancement et 'Player' est utilisé comme pseudo.")]
    [SerializeField] private bool openChatOnStart = true;

    [Header("Références UI")]
    [SerializeField] private Transform ongletContainer;
    [SerializeField] private Button tabButtonPrefab;
    [SerializeField] private Transform panelsContainer;
    [SerializeField] private GameObject chatPanelPrefab;
    [SerializeField] private InputField chatMessage;

    [Header("Global Chat")]
    [SerializeField] private GameObject globalPanel;
    [SerializeField] private Text globalChatHistoryText;
    [SerializeField] private Scrollbar globalScrollbar;

    [Header("Configuration")]
    [SerializeField, Tooltip("Nombre max de caractères par message")]
    private int maxMessageLength = 200;

    [Header("Simulés (optionnel)")]
    [Tooltip("Noms de joueurs simulés pour tester les MP.")]
    [SerializeField] private List<string> otherPlayerNames = new List<string>();

    [Header("Local player")]
    public string localPlayerName = "";

    private readonly Dictionary<string, GameObject> chatPanels = new();
    private readonly Dictionary<string, Text> chatHistories = new();
    private readonly Dictionary<string, Scrollbar> chatScrollbars = new();
    private readonly Dictionary<string, Button> tabButtons = new();
    private string currentTab = "Global";

    private bool isInitialized = false;

    void Start()
    {
        Debug.Log("[ChatUI_Solo] Start called. activeSelf=" + gameObject.activeSelf + ", activeInHierarchy=" + gameObject.activeInHierarchy);

        // trace parent chain (vide si pas de parent)
        var p = transform.parent;
        string parentChain = "";
        while (p != null)
        {
            parentChain += p.name + (p.gameObject.activeSelf ? "(activeSelf)" : "(inactiveSelf)") + " -> ";
            p = p.parent;
        }
        Debug.Log("[ChatUI_Solo] Parent chain: " + parentChain);

        if (openChatOnStart)
        {
            localPlayerName = "Player";
            InitChat();
            // assure que le gameObject est actif
            if (!gameObject.activeSelf)
            {
                Debug.Log("[ChatUI_Solo] Forcing gameObject active.");
                gameObject.SetActive(true);
            }
            // ensure UI internals are enabled
            EnsureUIEnabled();
        }
        else
        {
            Debug.Log("[ChatUI_Solo] openChatOnStart false. Chat not auto-initialized.");
        }
    }

    void EnsureUIEnabled()
    {
        // active les panels de conteneur
        if (ongletContainer != null && !ongletContainer.gameObject.activeSelf)
        {
            ongletContainer.gameObject.SetActive(true);
            Debug.Log("[ChatUI_Solo] ongletContainer was inactive -> activated.");
        }
        if (panelsContainer != null && !panelsContainer.gameObject.activeSelf)
        {
            panelsContainer.gameObject.SetActive(true);
            Debug.Log("[ChatUI_Solo] panelsContainer was inactive -> activated.");
        }

        // assure que le panel global est actif
        if (globalPanel != null && !globalPanel.activeSelf)
        {
            globalPanel.SetActive(true);
            Debug.Log("[ChatUI_Solo] globalPanel was inactive -> activated.");
        }

        // active tous les Canvas enfants
        var canvases = GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            if (!c.enabled)
            {
                c.enabled = true;
                Debug.Log("[ChatUI_Solo] Enabled Canvas on " + c.gameObject.name);
            }
        }

        // répare les CanvasGroup (alpha/interactable/blocksRaycasts)
        var cgs = GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in cgs)
        {
            if (cg.alpha < 1f || !cg.interactable || !cg.blocksRaycasts)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                Debug.Log("[ChatUI_Solo] Fixed CanvasGroup on " + cg.gameObject.name);
            }
        }

        // active les GraphicRaycasters
        var grs = GetComponentsInChildren<GraphicRaycaster>(true);
        foreach (var gr in grs)
        {
            if (!gr.enabled)
            {
                gr.enabled = true;
                Debug.Log("[ChatUI_Solo] Enabled GraphicRaycaster on " + gr.gameObject.name);
            }
        }

        // rend le champ d'input utilisable
        if (chatMessage != null && !chatMessage.interactable)
        {
            chatMessage.interactable = true;
            Debug.Log("[ChatUI_Solo] chatMessage made interactable.");
        }
    }

    public void InitChat()
    {
        if (isInitialized) return;

        if (ongletContainer == null || panelsContainer == null || chatPanelPrefab == null || chatMessage == null ||
            globalPanel == null || globalChatHistoryText == null || globalScrollbar == null || tabButtonPrefab == null)
        {
            Debug.LogError("[ChatUI_Solo] Il manque une référence UI dans l'Inspector !");
            return;
        }

        chatPanels.Clear();
        chatHistories.Clear();
        chatScrollbars.Clear();
        tabButtons.Clear();

        chatPanels["Global"] = globalPanel;
        chatHistories["Global"] = globalChatHistoryText;
        chatScrollbars["Global"] = globalScrollbar;

        var btnGlobal = Instantiate(tabButtonPrefab, ongletContainer, false);
        btnGlobal.name = "Tab_Global";
        var txtPro = btnGlobal.GetComponentInChildren<TextMeshProUGUI>();
        if (txtPro != null) txtPro.text = "Global";
        else
        {
            var t = btnGlobal.GetComponentInChildren<Text>();
            if (t != null) t.text = "Global";
        }
        btnGlobal.onClick.AddListener(() => ShowTab("Global"));
        tabButtons["Global"] = btnGlobal;

        ShowTab("Global");
        chatMessage.onEndEdit.RemoveAllListeners();
        chatMessage.onEndEdit.AddListener(OnEndEdit);

        isInitialized = true;
        Debug.Log("[ChatUI_Solo] InitChat finished.");
    }

    void Update()
    {
        if (chatMessage != null && chatMessage.isFocused &&
           (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            SendMessageInternal();
        }
    }

    void ProcessLocalInput(string input, string senderName)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        if (input.Length > maxMessageLength + currentTab.Length + 3) return;

        if (input.StartsWith("/w ")) HandlePrivateMessageLocal(input, senderName);
        else if (input.StartsWith("/c ")) HandleCommerceMessageLocal(input.Substring(3).Trim(), senderName);
        else ReceiveGlobal(senderName, input.Trim());
    }

    void HandlePrivateMessageLocal(string message, string senderName)
    {
        var parts = message.Split(' ', 3);
        if (parts.Length < 3)
        {
            AppendToTab(currentTab, "[Système] Usage : /w Pseudo message");
            return;
        }

        var target = parts[1];
        var priv = parts[2];

        if (target == localPlayerName || otherPlayerNames.Contains(target))
        {
            if (!chatPanels.ContainsKey(target))
                CreateTab(target);
            AppendToTab(target, "[à " + target + "] " + priv);
        }
        else AppendToTab(currentTab, "[Système] Joueur '" + target + "' introuvable.");
    }

    void HandleCommerceMessageLocal(string content, string senderName)
    {
        const string tabName = "Commerce";
        if (!chatPanels.ContainsKey(tabName))
            CreateTab(tabName);
        ShowTab(tabName);
        AppendToTab(tabName, "[à Commerce] " + content);
    }

    void ReceiveGlobal(string playerName, string message)
    {
        var color = playerName == localPlayerName ? "red" : "blue";
        AppendToTab("Global", "<color=" + color + ">[" + playerName + "]</color> " + message);
    }

    void CreateTab(string tabName)
    {
        if (chatPanels.ContainsKey(tabName)) return;

        var btn = Instantiate(tabButtonPrefab, ongletContainer, false);
        btn.name = "Tab_" + tabName;
        var labelTMP = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (labelTMP != null) labelTMP.text = tabName;
        else
        {
            var label = btn.GetComponentInChildren<Text>();
            if (label) label.text = tabName;
        }

        var closeBtnTransform = btn.transform.Find("CloseButton");
        if (closeBtnTransform != null)
        {
            var closeBtn = closeBtnTransform.GetComponent<Button>();
            if (closeBtn != null)
                closeBtn.onClick.AddListener(() => CloseTab(tabName));
        }

        btn.onClick.AddListener(() => ShowTab(tabName));
        tabButtons[tabName] = btn;

        var panel = Instantiate(chatPanelPrefab, panelsContainer, false);
        panel.name = tabName;
        chatPanels[tabName] = panel;
        panel.SetActive(false);

        var historyTransform = panel.transform.Find("Scroll View/Viewport/Content/ChatHistory");
        Text historyText = historyTransform != null ? historyTransform.GetComponent<Text>() : panel.GetComponentInChildren<Text>();
        var scrollbar = panel.GetComponentInChildren<Scrollbar>();
        chatHistories[tabName] = historyText;
        chatScrollbars[tabName] = scrollbar;
    }

    void CloseTab(string tabName)
    {
        if (tabName == "Global") return;
        if (tabButtons.TryGetValue(tabName, out var btn))
        {
            Destroy(btn.gameObject);
            tabButtons.Remove(tabName);
        }
        if (chatPanels.TryGetValue(tabName, out var panel))
        {
            Destroy(panel.gameObject);
            chatPanels.Remove(tabName);
        }
        chatHistories.Remove(tabName);
        chatScrollbars.Remove(tabName);
        if (currentTab == tabName)
            ShowTab("Global");
    }

    void ShowTab(string tabName)
    {
        foreach (var kv in chatPanels)
            kv.Value.SetActive(kv.Key == tabName);
        foreach (var kv in tabButtons)
            kv.Value.interactable = kv.Key != tabName;
        currentTab = tabName;
    }

    void AppendToTab(string tabName, string message)
    {
        if (!chatHistories.ContainsKey(tabName)) return;
        var h = chatHistories[tabName];
        if (h == null) return;
        h.text += message + "\n";
        StartCoroutine(ScrollToBottom(tabName));
    }

    IEnumerator ScrollToBottom(string tabName)
    {
        yield return null;
        if (chatScrollbars.TryGetValue(tabName, out var sb) && sb != null)
            sb.value = 0;
    }

    void OnEnable()
    {
        Debug.Log("[ChatUI_Solo] OnEnable called. activeSelf=" + gameObject.activeSelf + ", activeInHierarchy=" + gameObject.activeInHierarchy);
    }

    void OnDisable()
    {
        Debug.Log("[ChatUI_Solo] OnDisable called. stack:\n" + System.Environment.StackTrace);
    }

    public void OnEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SendMessageInternal();
    }

    void SendMessageInternal()
    {
        if (chatMessage == null) return;
        if (string.IsNullOrWhiteSpace(chatMessage.text)) return;

        var raw = chatMessage.text.Trim();
        if (raw.Length > maxMessageLength)
        {
            raw = raw.Substring(0, maxMessageLength);
            AppendToTab(currentTab, "[Système] Message tronqué à " + maxMessageLength + " caractères.");
        }

        string input;
        if (currentTab == "Global") input = raw;
        else if (currentTab == "Commerce") input = "/c " + raw;
        else input = "/w " + currentTab + " " + raw;

        ProcessLocalInput(input, localPlayerName);
        chatMessage.text = "";
        chatMessage.ActivateInputField();
    }
}
