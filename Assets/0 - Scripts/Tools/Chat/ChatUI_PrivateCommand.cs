using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace Mirror.Examples.Chat
{
    public class ChatUI_PrivateCommand : NetworkBehaviour
    {
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
        [SerializeField, Tooltip("Nombre max de caractères par message")] private int maxMessageLength = 200;

        internal static string localPlayerName;
        internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new();
        internal static readonly Dictionary<string, NetworkConnectionToClient> nameToConn = new();

        private readonly Dictionary<string, GameObject> chatPanels = new();
        private readonly Dictionary<string, Text> chatHistories = new();
        private readonly Dictionary<string, Scrollbar> chatScrollbars = new();
        private readonly Dictionary<string, Button> tabButtons = new();
        private string currentTab = "Global";

        public override void OnStartServer()
        {
            connNames.Clear();
            nameToConn.Clear();
        }

        public override void OnStartClient()
        {
            if (ongletContainer == null || panelsContainer == null || chatPanelPrefab == null || chatMessage == null || globalPanel == null || globalChatHistoryText == null || globalScrollbar == null || tabButtonPrefab == null)
            {
                Debug.LogError("[ChatUI] Il manque une référence UI dans l’Inspector !");
                return;
            }

            foreach (var kv in tabButtons)
            {
                if (kv.Key != "Global")
                    Destroy(kv.Value.gameObject);
            }
            foreach (var kv in chatPanels)
            {
                if (kv.Key != "Global")
                    Destroy(kv.Value);
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
            btnGlobal.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Global";
            btnGlobal.onClick.AddListener(() => ShowTab("Global"));
            tabButtons["Global"] = btnGlobal;

            ShowTab("Global");
        }


        [Command(requiresAuthority = false)]
        void CmdSend(string message, NetworkConnectionToClient sender = null)
        {
            if (message.Length > maxMessageLength + currentTab.Length + 3)
                return;

            string senderName = "Inconnu";
            if (sender?.identity != null)
            {
                var cp = sender.identity.GetComponent<ChatPlayer>();
                if (cp != null && !string.IsNullOrEmpty(cp.playerName))
                    senderName = cp.playerName;
            }

            // Mémorisation connexion
            if (!connNames.ContainsKey(sender))
            {
                connNames[sender] = senderName;
                nameToConn[senderName] = sender;
            }

            if (message.StartsWith("/w "))
            {
                HandlePrivateMessage(message, sender);
            }
            else if (message.StartsWith("/c "))
            {
                HandleCommerceMessage(message.Substring(3).Trim(), senderName, sender);
            }
            else
            {
                RpcReceiveGlobal(senderName, message.Trim());
            }
        }

        void HandlePrivateMessage(string message, NetworkConnectionToClient sender)
        {
            var parts = message.Split(' ', 3);
            if (parts.Length < 3)
            {
                if (isLocalPlayer)
                    AppendToTab(currentTab, "[Système] Usage : /w Pseudo message");
                return;
            }
            var target = parts[1];
            var priv = parts[2];
            if (nameToConn.TryGetValue(target, out var tgt))
            {
                if (tgt != sender)
                    TargetReceivePrivate(tgt, sender.identity.GetComponent<ChatPlayer>().playerName, priv, false);
                TargetReceivePrivate(sender, target, priv, true);
            }
            else if (connectionToClient is NetworkConnectionToClient localConn)
            {
                TargetReceivePrivate(localConn, "Système", $"Joueur « {target} » introuvable.", true);
            }
        }

        void HandleCommerceMessage(string content, string senderName, NetworkConnectionToClient sender)
        {
            // Envoi à tous clients : chacun reçoit le message, mais seuls les senders ouvrent l'onglet
            foreach (var kv in connNames)
            {
                var conn = kv.Key;
                bool isSenderConn = conn == sender;
                TargetReceiveCommerce(conn, senderName, content, isSenderConn);
            }
        }

        [ClientRpc]
        void RpcReceiveGlobal(string playerName, string message)
        {
            AppendToTab("Global", $"<color={(playerName == localPlayerName ? "red" : "blue")}>[{playerName}]</color> {message}");
        }

        [TargetRpc]
        void TargetReceivePrivate(NetworkConnection target, string otherName, string message, bool isSender)
        {
            if (!chatPanels.ContainsKey(otherName))
                CreateTab(otherName);
            ShowTab(otherName);

            var prefix = isSender ? $"[à {otherName}]" : $"[de {otherName}]";
            AppendToTab(otherName, $"{prefix} {message}");
        }

        [TargetRpc]
        void TargetReceiveCommerce(NetworkConnection target, string senderName, string message, bool isSender)
        {
            const string tabName = "Commerce";

            if (isSender)
            {
                // Pour l'émetteur : créer et ouvrir l'onglet
                if (!chatPanels.ContainsKey(tabName))
                    CreateTab(tabName);
                ShowTab(tabName);
            }
            else
            {
                // Pour les autres : n'ouvrir l'onglet que si déjà créé manuellement
                if (!chatPanels.ContainsKey(tabName))
                    return;
            }

            var prefix = isSender ? $"[à Commerce]" : $"[de {senderName}]";
            AppendToTab(tabName, $"{prefix} {message}");
        }

        void CreateTab(string tabName)
        {
            if (chatPanels.ContainsKey(tabName)) return;
            var btn = Instantiate(tabButtonPrefab, ongletContainer, false);
            btn.name = "Tab_" + tabName;
            var label = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            label.text = tabName;
            var closeBtn = btn.transform.Find("CloseButton")?.GetComponent<Button>();
            if (closeBtn != null)
                closeBtn.onClick.AddListener(() => CloseTab(tabName));
            btn.onClick.AddListener(() => ShowTab(tabName));
            tabButtons[tabName] = btn;

            var panel = Instantiate(chatPanelPrefab, panelsContainer, false);
            panel.name = tabName;
            chatPanels[tabName] = panel;
            panel.SetActive(false);

            var history = panel.transform
                .Find("Scroll View/Viewport/Content/ChatHistory")
                .GetComponent<Text>();
            var scrollbar = panel.GetComponentInChildren<Scrollbar>();
            chatHistories[tabName] = history;
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
            chatHistories[tabName].text += message + "\n";
            StartCoroutine(ScrollToBottom(tabName));
        }

        IEnumerator ScrollToBottom(string tabName)
        {
            yield return null;
            if (chatScrollbars.TryGetValue(tabName, out var sb))
                sb.value = 0;
        }

        public void OnEndEdit(string input)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SendMessage();
        }

        void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(chatMessage.text)) return;

            var raw = chatMessage.text.Trim();
            if (raw.Length > maxMessageLength)
            {
                raw = raw.Substring(0, maxMessageLength);
                AppendToTab(currentTab, $"[Système] Message tronqué à {maxMessageLength} caractères.");
            }

            string input;
            if (currentTab == "Global")
                input = raw;
            else if (currentTab == "Commerce")
                input = $"/c {raw}";
            else
                input = $"/w {currentTab} {raw}";

            CmdSend(input);
            chatMessage.text = "";
            chatMessage.ActivateInputField();
        }
    }
}
