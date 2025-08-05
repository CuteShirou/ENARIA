using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_ControllerLocal : MonoBehaviour
{
    private Entity_StatistiqueCombat stats;
    private NetworkIdentity net;

    private GameObject combatUI;
    private GameObject explorationUI;
    private Button readyButton;

    void Start()
    {
        net = GetComponent<NetworkIdentity>();
        if (net == null || !net.isLocalPlayer)
        {
            enabled = false;
            return;
        }

        stats = GetComponent<Entity_StatistiqueCombat>();
        if (stats == null)
        {
            Debug.LogError("[UI_ControllerLocal] Entity_StatistiqueCombat manquant !");
            enabled = false;
            return;
        }

        // 🔍 Recherche des Canvas
        combatUI = GameObject.Find("CombatUI");
        explorationUI = GameObject.Find("ExplorationUI");

        if (combatUI == null || explorationUI == null)
        {
            Debug.LogError("[UI_ControllerLocal] CombatUI ou ExplorationUI non trouvé !");
            enabled = false;
            return;
        }

        // 🔘 Recherche du bouton READY dans le CombatUI
        readyButton = combatUI.transform.Find("ReadyButton")?.GetComponent<Button>();

        if (readyButton == null)
        {
            Debug.LogWarning("[UI_ControllerLocal] ReadyButton introuvable dans CombatUI !");
        }
        else
        {
            readyButton.onClick.AddListener(OnClickReadyButton);
        }

        // Appliquer état initial
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (combatUI == null || explorationUI == null || stats == null) return;

        bool isFight = stats.isFight;

        // Assure qu’un seul des deux est actif
        if (combatUI.activeSelf != isFight)
            combatUI.SetActive(isFight);

        if (explorationUI.activeSelf != !isFight)
            explorationUI.SetActive(!isFight);
    }

    private void OnClickReadyButton()
    {
        if (stats != null && stats.isOwned)
        {
            stats.CmdToggleReady();
            Debug.Log("[UI_ControllerLocal] Bouton READY cliqué → CmdToggleReady()");
        }
    }
}
