using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;

[AddComponentMenu("Combat/Phase - End Combat (Local)")]
public class Phase_EndCombat : MonoBehaviour
{
    [Header("UI (scène unique)")]
    [SerializeField] private GameObject explorationUIRoot; // Exploration_UI
    [SerializeField] private GameObject combatUIRoot;      // Combat_UI
    [SerializeField] private bool autoFindUIsIfNull = true;
    [SerializeField] private string explorationUIObjectName = "Exploration_UI";
    [SerializeField] private string combatUIObjectName = "Combat_UI";

    [Header("Arena parents (containers)")]
    [SerializeField] private Transform teamRedParent;   
    [SerializeField] private Transform obstaclesParent;

    [Header("Popup résultat (dans Exploration_UI)")]
    [SerializeField] private GameObject resultPopupRoot;
    [SerializeField] private TMP_Text resultPopupText;
    [SerializeField] private string winText = "Vous avez GAGNE le combat";
    [SerializeField] private string loseText = "Vous avez PERDU le combat";

    private Combat_PhaseManager manager;

    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        // UI: Exploration ON, Combat OFF
        if (combatUIRoot) combatUIRoot.SetActive(false);
        if (explorationUIRoot) explorationUIRoot.SetActive(true);

        // Parents d’arène

        // === ORDRE : Dicos → Monstres/Obstacles → Joueurs → Grille ===

        // 1) Clear dictionnaires
        if (manager.tileGrid != null)
            manager.tileGrid.UnregisterAllEntities();

        // 2) Clear monstres & obstacles
        if (teamRedParent) DestroyAllChildren(teamRedParent);
        if (obstaclesParent) DestroyAllChildren(obstaclesParent);

        // 2.b) Joueurs → retour exploration + mode exploration
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

        // 3) Clear cases & grille
        if (manager.tileGrid != null)
            manager.tileGrid.ClearGrid(true);

        ShowResultPopup(manager.lastCombatWinning);
        Debug.Log($"[End] Combat terminé. Résultat: {(manager.lastCombatWinning ? "WIN" : "LOSE")}");
    }

    private void ReturnPlayerToExploration(GameObject player)
    {
        if (player == null) return;

        if (manager.phaseEnter != null && manager.phaseEnter.teamGreenParent != null &&
            player.transform.parent == manager.phaseEnter.teamGreenParent)
        {
            player.transform.SetParent(null, true);
        }

        var info = player.GetComponent<Entity_Info>();
        if (info != null)
        {
            RestorePlayerPosition(player, info.savePosEntity);
            RestorePlayerCameraConstraint(player, info.saveCamEntity);
        }

        // ➜ Mode Exploration via ScriptManager
        var sm = player.GetComponent<Player_ScriptManager>();
        if (sm) sm.SetExploration();

        player.SendMessage("OnCombatEnd", manager.lastCombatWinning, SendMessageOptions.DontRequireReceiver);
    }

    private void RestorePlayerPosition(GameObject player, Vector3 savedPos)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = savedPos;
        if (cc != null) cc.enabled = true;
    }

    // Re-sélectionne la source du ParentConstraint de la caméra DU JOUEUR
    private void RestorePlayerCameraConstraint(GameObject player, string targetSourceName)
    {
        if (string.IsNullOrWhiteSpace(targetSourceName)) return;

        var playerCam = player.GetComponentInChildren<Camera>(true);
        if (playerCam == null) { Debug.LogWarning("[End] Caméra enfant du joueur introuvable."); return; }

        var constraint = playerCam.GetComponent<ParentConstraint>();
        if (constraint == null) { Debug.LogWarning("[End] ParentConstraint introuvable sur la caméra du joueur."); return; }

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
            if (go == null) continue;
#if UNITY_EDITOR
            if (immediate) DestroyImmediate(go);
            else
#endif
                Destroy(go);
        }
    }

    private void ShowResultPopup(bool win)
    {
        if (resultPopupRoot != null)
        {
            resultPopupRoot.SetActive(true);
            if (resultPopupText != null)
                resultPopupText.text = win ? winText : loseText;
        }
    }

    public void OnClick_CloseResultPopup()
    {
        if (resultPopupRoot != null)
            resultPopupRoot.SetActive(false);
    }
}
