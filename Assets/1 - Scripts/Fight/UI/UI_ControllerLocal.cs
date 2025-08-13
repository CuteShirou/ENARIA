using UnityEngine;
using UnityEngine.UI;
using Mirror;

//----------------------------------------------------------
// UI_ControllerLocal
// - NetworkBehaviour initialisé au bon moment via OnStartAuthority().
// - Ne pilote l'UI QUE pour le joueur local (ownership).
// - Retrouve les Canvas même s'ils sont inactifs.
// - Bascule Exploration/Combat selon stats.isFight.
// - Ready -> CmdToggleReady(), Quit -> Player_CombatExit.CmdRequestAbandon().
//----------------------------------------------------------
public class UI_ControllerLocal : NetworkBehaviour
{
    // Réseau / données
    private Entity_StatistiqueCombat stats;      // SyncVars (isFight/isReady) sur le player

    // UI (résolues dynamiquement)
    private GameObject combatUI;                 // "CombatUI"
    private GameObject explorationUI;            // "ExplorationUI"
    private Button readyButton;                  // "CombatUI/ReadyButton"
    private Button quitButton;                   // "CombatUI/QuitButton"

    // Garde-fous de binding
    private bool readyBound = false;
    private bool quitBound = false;

    //------------------------------------------------------------------------
    // OnStartAuthority : appelé quand CE client devient propriétaire de ce player
    // → C’est le BON moment pour initialiser l’UI locale.
    //------------------------------------------------------------------------
    public override void OnStartAuthority()
    {
        // Récupère les stats sur CE player
        stats = GetComponent<Entity_StatistiqueCombat>();

        // Résout les références UI (même inactives) et bind les boutons
        ResolveUIReferences();
        BindButtonsIfPossible();

        // Force une première bascule d’UI
        UpdateUIVisibility();

        // S'assure que l'Update tourne pour le local uniquement
        enabled = true;
    }

    //------------------------------------------------------------------------
    // OnStartClient : pour les players distants, on désactive ce script.
    // NOTE : on teste l’ownership via netIdentity.isOwned (API Mirror récente).
    //------------------------------------------------------------------------
    public override void OnStartClient()
    {
        if (netIdentity == null || !netIdentity.isOwned)
        {
            enabled = false; // joueur non local → ne rien piloter
        }
    }

    //------------------------------------------------------------------------
    // Update : exécute UNIQUEMENT pour le joueur local (enabled=false sinon)
    // - Répare les références si elles arrivent tard (UI instanciées/activées après).
    // - Bascule l’affichage selon stats.isFight.
    //------------------------------------------------------------------------
    private void Update()
    {
        // Si des refs se perdent (changements de scène / activation tardive), on re-résout
        if (combatUI == null || explorationUI == null || readyButton == null || quitButton == null)
        {
            ResolveUIReferences();
            BindButtonsIfPossible();
        }

        UpdateUIVisibility();
    }

    //------------------------------------------------------------------------
    // ResolveUIReferences : retrouve CombatUI/ExplorationUI et leurs boutons,
    // même s'ils sont inactifs (Resources.FindObjectsOfTypeAll).
    //------------------------------------------------------------------------
    private void ResolveUIReferences()
    {
        if (combatUI == null) combatUI = FindInSceneIncludingInactive("CombatUI");
        if (explorationUI == null) explorationUI = FindInSceneIncludingInactive("ExplorationUI");

        if (combatUI != null)
        {
            if (readyButton == null)
            {
                Transform t = combatUI.transform.Find("ReadyButton");
                if (t != null) readyButton = t.GetComponent<Button>();
            }

            if (quitButton == null)
            {
                Transform t = combatUI.transform.Find("QuitButton");
                if (t != null) quitButton = t.GetComponent<Button>();
            }
        }
    }

    // Recherche un GameObject par nom dans la scène, même s'il est inactif.
    private GameObject FindInSceneIncludingInactive(string name)
    {
        // ATTENTION : inclut aussi des objets “cachés” de la scène, mais pas les assets hors scène.
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (!go.scene.IsValid()) continue;      // ignore les assets/prefabs non instanciés en scène
            if (go.name == name) return go;
        }
        return null;
    }

    //------------------------------------------------------------------------
    // BindButtonsIfPossible : branche (une seule fois) les listeners Ready/Quit
    //------------------------------------------------------------------------
    private void BindButtonsIfPossible()
    {
        if (readyButton != null && !readyBound)
        {
            readyButton.onClick.RemoveListener(OnClickReadyButton);
            readyButton.onClick.AddListener(OnClickReadyButton);
            readyBound = true;
        }

        if (quitButton != null && !quitBound)
        {
            quitButton.onClick.RemoveListener(OnClickQuitButton);
            quitButton.onClick.AddListener(OnClickQuitButton);
            quitBound = true;
        }
    }

    //------------------------------------------------------------------------
    // UpdateUIVisibility : bascule ExplorationUI/CombatUI en fonction de isFight
    //------------------------------------------------------------------------
    private void UpdateUIVisibility()
    {
        if (stats == null) return;

        bool isFight = stats.isFight;

        // CombatUI = actif si en combat
        if (combatUI != null && combatUI.activeSelf != isFight)
            combatUI.SetActive(isFight);

        // ExplorationUI = actif si pas en combat
        if (explorationUI != null && explorationUI.activeSelf != !isFight)
            explorationUI.SetActive(!isFight);
    }

    //------------------------------------------------------------------------
    // OnDisable : nettoyage des listeners quand ce script est désactivé
    //------------------------------------------------------------------------
    private void OnDisable()
    {
        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnClickReadyButton);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnClickQuitButton);

        readyBound = false;
        quitBound = false;
    }

    //------------------------------------------------------------------------
    // OnClickReadyButton : toggle serveur de l'état "prêt"
    //------------------------------------------------------------------------
    private void OnClickReadyButton()
    {
        if (stats != null)
            stats.CmdToggleReady(); // Command Mirror exécutée sur le serveur
        Debug.Log("[UI_ControllerLocal] ReadyButton -> CmdToggleReady()");
    }

    //------------------------------------------------------------------------
    // OnClickQuitButton : abandon (server-authority) via Player_CombatExit
    //------------------------------------------------------------------------
    private void OnClickQuitButton()
    {
        var exit = GetComponent<Player_CombatExit>();
        if (exit == null)
        {
            Debug.LogWarning("[UI_ControllerLocal] Player_CombatExit manquant sur le Player.");
            return;
        }

        exit.CmdRequestAbandon(); // Command Mirror exécutée sur le serveur
        Debug.Log("[UI_ControllerLocal] QuitButton -> CmdRequestAbandon()");
    }
}
