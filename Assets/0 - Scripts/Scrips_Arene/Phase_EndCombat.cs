using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations;

[AddComponentMenu("Combat/Phase - End Combat (Local)")]
public class Phase_EndCombat : MonoBehaviour
{
    [Header("UI (références scène)")]
    [SerializeField] private GameObject explorationUIRoot;
    [SerializeField] private GameObject combatUIRoot;

    [Header("Parents arène")]
    [SerializeField] private Transform teamRedParent;
    [SerializeField] private Transform obstaclesParent;

    [Header("Popup résultat")]
    [SerializeField] private GameObject resultPopupRoot;   // Panel_Popup_EndCombat
    [SerializeField] private TMP_Text resultPopupText;     // Title_Result_EndFight
    [SerializeField] private string winText = "Vous avez GAGNÉ le combat";
    [SerializeField] private string loseText = "Vous avez PERDU le combat";

    [Header("Win/Lose UI")]
    [SerializeField] private Transform contentWin;         // .../Panel_Team_Win/.../Content
    [SerializeField] private Transform contentLose;        // .../Panel_Team_Lose/.../Content
    [SerializeField] private GameObject prefabLineWin;     // Prefab_Ligne_EndFight_Win (avec EndFight_LineUI)
    [SerializeField] private GameObject prefabLineLose;    // Prefab_Ligne_EndFight_Lose
    [SerializeField] private Sprite defaultIcon;

    private Combat_PhaseManager manager;

    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        // Bascule d'UI
        if (combatUIRoot) combatUIRoot.SetActive(false);
        if (explorationUIRoot) explorationUIRoot.SetActive(true);

        // Snapshot des équipes avant nettoyage
        var greenSnapshot = manager?.phaseEnter != null ? new List<GameObject>(manager.phaseEnter.greenTeam) : new List<GameObject>();
        var redSnapshot = manager?.phaseEnter != null ? new List<GameObject>(manager.phaseEnter.redTeam) : new List<GameObject>();

        // Affiche la pop-up + construit les panels
        ShowResultPopup(manager.lastCombatWinning);
        BuildWinLosePanels_PerPlayerDistribution(manager.winnerTeam, greenSnapshot, redSnapshot);

        // Nettoyage arène / retour joueurs
        if (manager.tileGrid != null)
            manager.tileGrid.UnregisterAllEntities();

        if (teamRedParent) DestroyAllChildren(teamRedParent);
        if (obstaclesParent) DestroyAllChildren(obstaclesParent);

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

    // Construit Win/Lose avec répartition de drops PAR joueur gagnant
    private void BuildWinLosePanels_PerPlayerDistribution(CombatTeamId winner, List<GameObject> green, List<GameObject> red)
    {
        ClearContainer(contentWin);
        ClearContainer(contentLose);

        // Gagnants / perdants
        List<GameObject> winners = winner == CombatTeamId.Green ? green : (winner == CombatTeamId.Red ? red : new List<GameObject>());
        List<GameObject> losers = winner == CombatTeamId.Green ? red : (winner == CombatTeamId.Red ? green : new List<GameObject>());

        // WIN : une ligne par gagnant + tirages indépendants
        if (contentWin && prefabLineWin)
        {
            foreach (var winnerEntity in winners)
            {
                var lineGO = CreateLineForEntity(winnerEntity, contentWin, prefabLineWin, isWinner: true);

                // 1) Calcule les drops pour CE joueur
                List<GameObject> dropsForThisWinner = ComputeDropsForOneWinner(losers);

                // 2) Ajoute les items correspondants à l'inventaire
                GiveItemsToInventory(dropsForThisWinner);

                // 3) Affiche ces drops dans la ligne UI
                var ui = lineGO ? lineGO.GetComponent<EndFight_LineUI>() : null;
                if (ui != null) ui.SetDrops(dropsForThisWinner);
            }
        }

        // LOSE : lignes simples
        if (contentLose && prefabLineLose)
        {
            foreach (var loserEntity in losers)
            {
                CreateLineForEntity(loserEntity, contentLose, prefabLineLose, isWinner: false);
            }
        }
    }

    // Calcule les drops pour UN gagnant à partir de tous les perdants
    private List<GameObject> ComputeDropsForOneWinner(List<GameObject> losers)
    {
        var drops = new List<GameObject>();
        if (losers == null || losers.Count == 0) return drops;

        foreach (var entity in losers)
        {
            if (!entity) continue;

            var info = entity.GetComponent<Entity_Info>();
            if (info == null || info.listDropRessources == null) continue;

            foreach (var entry in info.listDropRessources)
            {
                if (entry == null) continue;

                GameObject prefab = entry.ressourcePrefab;   // Prefab_DropRessource spécifique
                float chance = Mathf.Clamp(entry.dropChance, 0f, 100f);

                if (!prefab) continue;

                if (RollChance(chance))
                    drops.Add(prefab); // On garde le prefab (il contient l'Item via InventoryItemController)
            }
        }

        return drops;
    }

    // Ajoute chaque drop dans l'inventaire joueur
    private void GiveItemsToInventory(List<GameObject> dropPrefabs)
    {
        if (dropPrefabs == null || dropPrefabs.Count == 0) return;

        foreach (var prefab in dropPrefabs)
        {
            if (!prefab) continue;

            // Récupère l'Item depuis le prefab (via InventoryItemController)
            var ctrl = prefab.GetComponent<InventoryItemController>();
            Item item = ctrl != null ? ctrl.GetItem() : null;

            if (item != null)
            {
                // Ajoute dans le premier slot vide (selon ton utilitaire)
                InventoryUtil.AddItemToFirstEmpty(item);
            }
            else
            {
                Debug.LogWarning($"[End] Drop prefab sans Item lisible: {prefab.name}");
            }
        }
    }

    // Tirage sur un pourcentage [0..100]
    private bool RollChance(float percent)
    {
        return Random.Range(0f, 100f) < percent;
    }

    // Instancie une ligne et renseigne l'icône + le nom
    private GameObject CreateLineForEntity(GameObject entity, Transform parent, GameObject prefabLine, bool isWinner)
    {
        if (!entity || !parent || !prefabLine) return null;

        var go = Instantiate(prefabLine, parent, false);

        var icon = go.transform.Find("IconEntity")?.GetComponent<Image>();
        if (icon == null) icon = go.transform.Find("IconPlayer")?.GetComponent<Image>();
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

        return go;
    }

    // Lit icône et nom depuis l'entité (Entity_Info prioritaire)
    private void GetEntityDisplay(GameObject entity, out Sprite iconSprite, out string displayName)
    {
        iconSprite = null;
        displayName = entity ? entity.name : "";

        var info = entity ? entity.GetComponent<Entity_Info>() : null;
        if (info != null)
        {
            if (!string.IsNullOrWhiteSpace(info.entity_Name))
                displayName = info.entity_Name;
            if (info.entity_Icon != null)
                iconSprite = info.entity_Icon;
        }
    }

    private void ShowResultPopup(bool win)
    {
        if (!resultPopupRoot) return;
        resultPopupRoot.SetActive(true);
        if (resultPopupText) resultPopupText.text = win ? winText : loseText;
    }

    public void OnClick_CloseResultPopup()
    {
        if (resultPopupRoot) resultPopupRoot.SetActive(false);
    }

    // ----------------- Utilitaires -----------------

    private void ReturnPlayerToExploration(GameObject player)
    {
        if (!player) return;

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
