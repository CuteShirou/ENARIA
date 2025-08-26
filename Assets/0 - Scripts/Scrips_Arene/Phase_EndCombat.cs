using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations;

[AddComponentMenu("Combat/Phase - End Combat (Local)")]
public class Phase_EndCombat : MonoBehaviour
{
    [Header("UI (références scène)")]
    [SerializeField] private GameObject explorationUIRoot; // Glisser l'objet Exploration_UI
    [SerializeField] private GameObject combatUIRoot;      // Glisser l'objet Combat_UI

    [Header("Parents arène")]
    [SerializeField] private Transform teamRedParent;      // Conteneur des monstres
    [SerializeField] private Transform obstaclesParent;    // Conteneur des obstacles

    [Header("Popup résultat")]
    [SerializeField] private GameObject resultPopupRoot;   // Panel_Popup_EndCombat
    [SerializeField] private TMP_Text resultPopupText;     // Title_Result_EndFight
    [SerializeField] private string winText = "Vous avez GAGNÉ le combat";
    [SerializeField] private string loseText = "Vous avez PERDU le combat";

    [Header("Win/Lose UI")]
    [SerializeField] private Transform contentWin;         // .../Panel_Team_Win/.../Content
    [SerializeField] private Transform contentLose;        // .../Panel_Team_Lose/.../Content
    [SerializeField] private GameObject prefabLineWin;     // Prefab_Ligne_EndFight_Win
    [SerializeField] private GameObject prefabLineLose;    // Prefab_Ligne_EndFight_Lose
    [SerializeField] private Sprite defaultIcon;           // Icône par défaut si entité sans sprite

    private Combat_PhaseManager manager;

    // Appelée par le PhaseManager à l'entrée de la phase
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        // Bascule d'UI (aucun Find : tout est assigné dans l’Inspector)
        if (combatUIRoot) combatUIRoot.SetActive(false);
        if (explorationUIRoot) explorationUIRoot.SetActive(true);

        // Snapshot des équipes avant nettoyage (évite de perdre les données)
        var greenSnapshot = manager?.phaseEnter != null ? new List<GameObject>(manager.phaseEnter.greenTeam) : new List<GameObject>();
        var redSnapshot = manager?.phaseEnter != null ? new List<GameObject>(manager.phaseEnter.redTeam) : new List<GameObject>();

        // Affiche la pop-up et remplit les panels Win/Lose
        ShowResultPopup(manager.lastCombatWinning);
        BuildWinLosePanels(manager.winnerTeam, greenSnapshot, redSnapshot);

        // Nettoyage de l’arène
        if (manager.tileGrid != null)
            manager.tileGrid.UnregisterAllEntities();

        if (teamRedParent) DestroyAllChildren(teamRedParent);
        if (obstaclesParent) DestroyAllChildren(obstaclesParent);

        // Retour des joueurs et reset des listes
        if (manager.phaseEnter != null)
        {
            var players = new List<GameObject>(manager.phaseEnter.greenTeam);
            foreach (var player in players)
                ReturnPlayerToExploration(player);

            manager.phaseEnter.redTeam.Clear();
            manager.phaseEnter.greenTeam.Clear();
            manager.phaseEnter.AllFighters.Clear();
            manager.phaseEnter.SetMonsterState(MonsterState.InNature);
        }

        if (manager.tileGrid != null)
            manager.tileGrid.ClearGrid(true);

        Debug.Log($"[End] Combat terminé. Résultat: {(manager.lastCombatWinning ? "WIN" : "LOSE")}");
    }

    // Construit les panneaux Win et Lose
    private void BuildWinLosePanels(CombatTeamId winner, List<GameObject> green, List<GameObject> red)
    {
        ClearContainer(contentWin);
        ClearContainer(contentLose);

        // Sélectionne gagnants/perdants
        List<GameObject> winners = winner == CombatTeamId.Green ? green : (winner == CombatTeamId.Red ? red : new List<GameObject>());
        List<GameObject> losers = winner == CombatTeamId.Green ? red : (winner == CombatTeamId.Red ? green : new List<GameObject>());

        // Instancie une ligne par entité
        if (contentWin && prefabLineWin)
            foreach (var e in winners) CreateLineForEntity(e, contentWin, prefabLineWin);

        if (contentLose && prefabLineLose)
            foreach (var e in losers) CreateLineForEntity(e, contentLose, prefabLineLose);
    }

    // Crée une ligne et renseigne l'icône + le nom
    private void CreateLineForEntity(GameObject entity, Transform parent, GameObject prefabLine)
    {
        if (!entity || !parent || !prefabLine) return;

        var go = Instantiate(prefabLine, parent, false);

        // On reste dans la hiérarchie du prefab (transform.Find local est OK)
        var icon = go.transform.Find("IconEntity")?.GetComponent<Image>();
        if (icon == null) icon = go.transform.Find("IconPlayer")?.GetComponent<Image>(); // secours si ancien nom
        var nameText = go.transform.Find("Name_Text")?.GetComponent<TMP_Text>();

        GetEntityDisplay(entity, out Sprite iconSprite, out string displayName);

        if (icon)
        {
            icon.sprite = iconSprite ? iconSprite : defaultIcon;
            icon.enabled = (icon.sprite != null);
            icon.preserveAspect = true;
        }
        if (nameText)
            nameText.text = string.IsNullOrWhiteSpace(displayName) ? entity.name : displayName;
    }

    // Lit icône et nom depuis les composants de l'entité
    private void GetEntityDisplay(GameObject entity, out Sprite iconSprite, out string displayName)
    {
        iconSprite = null;
        displayName = entity ? entity.name : "";

        // Source principale: Entity_Info (entity_Name, entity_Icon)
        var info = entity ? entity.GetComponent<Entity_Info>() : null;
        if (info != null)
        {
            if (!string.IsNullOrWhiteSpace(info.entity_Name))
                displayName = info.entity_Name;

            if (info.entity_Icon != null)
                iconSprite = info.entity_Icon;
        }

        // Ajouter ici d’autres sources si nécessaire (stats, data sheet, etc.)
    }

    // Active la pop-up + texte
    private void ShowResultPopup(bool win)
    {
        if (!resultPopupRoot) return;
        resultPopupRoot.SetActive(true);
        if (resultPopupText) resultPopupText.text = win ? winText : loseText;
    }

    // Bouton fermer (appelé depuis l’UI)
    public void OnClick_CloseResultPopup()
    {
        if (resultPopupRoot) resultPopupRoot.SetActive(false);
    }

    // ----------------- Utilitaires -----------------

    // Replace le joueur en exploration et restaure ses réglages
    private void ReturnPlayerToExploration(GameObject player)
    {
        if (!player) return;

        // Détache du parent d'équipe si besoin
        if (manager.phaseEnter != null && manager.phaseEnter.teamGreenParent != null &&
            player.transform.parent == manager.phaseEnter.teamGreenParent)
        {
            player.transform.SetParent(null, true);
        }

        // Restaure position/caméra depuis Entity_Info
        var info = player.GetComponent<Entity_Info>();
        if (info != null)
        {
            RestorePlayerPosition(player, info.savePosEntity);
            RestorePlayerCameraConstraint(player, info.saveCamEntity);
        }

        // Repasse le contrôleur en mode exploration
        var sm = player.GetComponent<Player_ScriptManager>();
        if (sm) sm.SetExploration();

        // Hook optionnel si d'autres systèmes écoutent la fin de combat
        player.SendMessage("OnCombatEnd", manager.lastCombatWinning, SendMessageOptions.DontRequireReceiver);
    }

    private void RestorePlayerPosition(GameObject player, Vector3 savedPos)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = savedPos;
        if (cc != null) cc.enabled = true;
    }

    private void RestorePlayerCameraConstraint(GameObject player, string targetSourceName)
    {
        if (string.IsNullOrWhiteSpace(targetSourceName)) return;

        var playerCam = player.GetComponentInChildren<Camera>(true);
        if (!playerCam) { Debug.LogWarning("[End] Caméra enfant du joueur introuvable."); return; }

        var constraint = playerCam.GetComponent<ParentConstraint>();
        if (!constraint) { Debug.LogWarning("[End] ParentConstraint introuvable sur la caméra du joueur."); return; }

        bool found = false;
        for (int i = 0; i < constraint.sourceCount; i++)
        {
            var src = constraint.GetSource(i);
            bool match = (src.sourceTransform != null && src.sourceTransform.name == targetSourceName);
            src.weight = match ? 1f : 0f;
            constraint.SetSource(i, src);
            if (match) found = true;
        }

        if (!found)
            Debug.LogWarning($"[End] Source '{targetSourceName}' non trouvée dans le ParentConstraint de la caméra joueur.");
    }

    // Détruit tous les enfants d’un conteneur
    private void DestroyAllChildren(Transform root)
    {
        var toDestroy = new List<GameObject>();
        foreach (Transform child in root)
            if (child != null) toDestroy.Add(child.gameObject);

#if UNITY_EDITOR
        bool immediate = !Application.isPlaying;
#endif
        foreach (var go in toDestroy)
        {
            if (!go) continue;
#if UNITY_EDITOR
            if (immediate) DestroyImmediate(go);
            else
#endif
                Destroy(go);
        }
    }

    // Vide un container UI (pour re-remplir proprement)
    private void ClearContainer(Transform container)
    {
        if (!container) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var c = container.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(c.gameObject);
            else
#endif
                Destroy(c.gameObject);
        }
    }
}
