using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations;
using System.Globalization; // Pour le formatage 3 par 3

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

    [Header("Runtime")]
    [SerializeField] private int lastTotalXpGained = 0;    // XP total gagné par l'équipe gagnante (mémorisé)
    public int LastTotalXpGained => lastTotalXpGained;     // Getter public pour usage ultérieur

    [Header("Timing d'attente (VFX/Animations)")]
    [SerializeField] private float graceDelayBeforeCheck = 0.05f;   // Petit délai pour laisser partir les derniers triggers
    [SerializeField] private float maxWaitPopups = 3f;               // Timeout pour les pop-ups de dégâts
    [SerializeField] private float maxWaitAnimations = 5f;           // Timeout pour les anims Hit/Death
    [SerializeField] private string[] animatorBusyStateNames = new[] { "Hit", "Death", "PlayHit", "PlayDeath" }; // Noms d'états considérés "occupés"
    [SerializeField] private string[] animatorBusyTags = new[] { "Hit", "Death" };                                // Tags d'états considérés "occupés"
    [SerializeField] private bool debugLogs = false;                  // Active quelques logs de debug

    private Combat_PhaseManager manager;

    // ---------------------------------------------------------
    // InitPhase : point d'entrée de la phase de fin de combat
    //   Désormais lance une coroutine qui attend la fin des VFX/animations,
    //   puis exécute le traitement existant (UI, résultats, nettoyage).
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        // Lancement asynchrone pour laisser finir visuels et animations
        StopAllCoroutines();
        StartCoroutine(Co_InitPhase());
    }

    // ---------------------------------------------------------
    // Co_InitPhase : pipeline d'attente puis finalisation
    private IEnumerator Co_InitPhase()
    {
        // Petit délai de grâce pour laisser partir le dernier frame d'impact
        if (graceDelayBeforeCheck > 0f)
            yield return new WaitForSecondsRealtime(graceDelayBeforeCheck);

        // 1) Attendre la fin des pop-ups (dégâts/PA/PM)
        yield return StartCoroutine(Co_WaitDamagePopups());

        // 2) Attendre la fin des animations Hit/Death en cours
        yield return StartCoroutine(Co_WaitEntityAnimations());

        // 3) Exécuter ensuite l'ancienne logique de fin (inchangée dans son intention)
        FinalizeEndCombat();
    }

    // ---------------------------------------------------------
    // Co_WaitDamagePopups : attend qu'il n'y ait plus de pop-ups actives
    private IEnumerator Co_WaitDamagePopups()
    {
        float elapsed = 0f;

        while (elapsed < maxWaitPopups)
        {
            int active = GetActivePopupCount();
            if (active <= 0) break;

            if (debugLogs) Debug.Log($"[End][WaitPopups] Actives={active} (t={elapsed:0.00}s)");
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // ---------------------------------------------------------
    // GetActivePopupCount : compte les pop-ups encore présentes et actives
    private int GetActivePopupCount()
    {
        // On compte les instances actives en scène.
        // Astuce simple et robuste sans dépendre d'un compteur statique.
        var all = FindObjectsOfType<Popup_DisplayNumber>(true);
        int count = 0;
        for (int i = 0; i < all.Length; i++)
        {
            var p = all[i];
            if (p != null && p.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }

    // ---------------------------------------------------------
    // Co_WaitEntityAnimations : attend que les anims Hit/Death soient finies
    private IEnumerator Co_WaitEntityAnimations()
    {
        // On collecte les Animator des entités encore connues du combat
        List<Animator> animators = CollectAnimatorsFromFighters();

        float elapsed = 0f;
        while (elapsed < maxWaitAnimations)
        {
            bool anyBusy = false;

            for (int i = 0; i < animators.Count; i++)
            {
                var a = animators[i];
                if (!a || !a.gameObject.activeInHierarchy) continue;

                if (IsAnimatorBusy(a))
                {
                    anyBusy = true;
                    break;
                }
            }

            if (!anyBusy) break;

            if (debugLogs) Debug.Log($"[End][WaitAnims] Animations en cours (t={elapsed:0.00}s)");
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // ---------------------------------------------------------
    // CollectAnimatorsFromFighters : récupère les Animator de toutes les entités listées côté phaseEnter
    private List<Animator> CollectAnimatorsFromFighters()
    {
        var list = new List<Animator>();

        var fighters = manager != null && manager.phaseEnter != null ? manager.phaseEnter.AllFighters : null;
        if (fighters != null)
        {
            for (int i = 0; i < fighters.Count; i++)
            {
                var go = fighters[i];
                if (!go) continue;

                // On prend l'Animator sur l'entité (ou ses enfants)
                var animator = go.GetComponentInChildren<Animator>(true);
                if (animator != null && !list.Contains(animator))
                    list.Add(animator);
            }
        }

        return list;
    }

    // ---------------------------------------------------------
    // IsAnimatorBusy : détecte un état "occupé" (Hit/Death) par nom ou tag
    private bool IsAnimatorBusy(Animator animator)
    {
        if (!animator) return false;

        // Si en transition, on considère que l'anim n'est pas totalement finie
        if (animator.IsInTransition(0))
            return true;

        var st = animator.GetCurrentAnimatorStateInfo(0);

        // Par tags d'abord (plus robuste si tes states sont taggés)
        for (int i = 0; i < animatorBusyTags.Length; i++)
        {
            string tag = animatorBusyTags[i];
            if (!string.IsNullOrEmpty(tag) && st.IsTag(tag))
                return true;
        }

        // Par nom d'état (souvent "Hit", "Death", etc.)
        for (int i = 0; i < animatorBusyStateNames.Length; i++)
        {
            string name = animatorBusyStateNames[i];
            if (string.IsNullOrEmpty(name)) continue;

            // On teste le nom simple et avec "Base Layer." par sécurité
            if (st.IsName(name) || st.IsName("Base Layer." + name))
                return true;
        }

        // Sinon on considère l'anim OK (Idle/Locomotion/etc.)
        return false;
    }

    // ---------------------------------------------------------
    // FinalizeEndCombat : reprend ton traitement initial de fin de combat
    private void FinalizeEndCombat()
    {
        // Désactive l'UI combat et réactive l'UI exploration
        if (combatUIRoot) combatUIRoot.SetActive(false);
        if (explorationUIRoot) explorationUIRoot.SetActive(true);

        // Par sécurité, si des pop-ups résiduelles existent encore, on purge
        Popup_DisplayNumber.DestroyAllActivePopups();

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
            for (int i = 0; i < players.Count; i++)
                ReturnPlayerToExploration(players[i]);

            manager.phaseEnter.redTeam.Clear();
            manager.phaseEnter.greenTeam.Clear();
            manager.phaseEnter.AllFighters.Clear();
            manager.phaseEnter.SetMonsterState(MonsterState.InNature);
        }

        if (manager.tileGrid != null)
            manager.tileGrid.ClearGrid(true);

        Debug.Log($"[End] Combat terminé. Résultat: {(manager.lastCombatWinning ? "WIN" : "LOSE")}");
    }

    // ---------------------------------------------------------
    // BuildWinLosePanels_PerPlayerDistribution : construit Win/Lose par joueur gagnant
    private void BuildWinLosePanels_PerPlayerDistribution(CombatTeamId winner, List<GameObject> green, List<GameObject> red)
    {
        ClearContainer(contentWin);
        ClearContainer(contentLose);

        List<GameObject> winners = winner == CombatTeamId.Green ? green : (winner == CombatTeamId.Red ? red : new List<GameObject>());
        List<GameObject> losers = winner == CombatTeamId.Green ? red : (winner == CombatTeamId.Red ? green : new List<GameObject>());

        lastTotalXpGained = ComputeTotalXpFromLosers(losers);

        if (winner == CombatTeamId.Green && lastTotalXpGained > 0)
        {
            AwardXpToWinners(winners, lastTotalXpGained);
        }

        if (contentWin && prefabLineWin)
        {
            for (int i = 0; i < winners.Count; i++)
            {
                var winnerEntity = winners[i];
                var lineGO = CreateLineForEntity(winnerEntity, contentWin, prefabLineWin, true);

                List<GameObject> dropsForThisWinner = ComputeDropsForOneWinner(losers);
                GiveItemsToInventory(dropsForThisWinner);

                var ui = lineGO ? lineGO.GetComponent<EndFight_LineUI>() : null;
                if (ui != null) ui.SetDrops(dropsForThisWinner);

                var xpText = lineGO ? lineGO.transform.Find("Gain_XpBar")?.GetComponent<TMP_Text>() : null;
                if (xpText != null)
                    xpText.text = $"+ {FormatNumberGrouped(lastTotalXpGained)} xp";
            }
        }

        if (contentLose && prefabLineLose)
        {
            for (int i = 0; i < losers.Count; i++)
                CreateLineForEntity(losers[i], contentLose, prefabLineLose, false);
        }
    }

    // ---------------------------------------------------------
    // AwardXpToWinners : crédite l'XP aux gagnants qui sont des joueurs
    private void AwardXpToWinners(List<GameObject> winners, int xpAmount)
    {
        if (winners == null || winners.Count == 0) return;
        if (xpAmount <= 0) return;

        for (int i = 0; i < winners.Count; i++)
        {
            var entity = winners[i];
            if (!entity) continue;

            if (!IsPlayer(entity)) continue;

            var info = entity.GetComponent<Entity_Info>();
            if (info != null)
            {
                info.GainExperience(xpAmount);
                Debug.Log($"[End][XP] +{xpAmount} xp -> {entity.name} (niveau {info.entity_Level}, reste {info.experience} / {info.GetExperienceToNextLevel()}).");
            }
        }
    }

    // ---------------------------------------------------------
    // IsPlayer : détermine si l'entité est un joueur (et non un monstre)
    private bool IsPlayer(GameObject entity)
    {
        return entity != null && entity.GetComponent<Player_ScriptManager>() != null;
    }

    // ---------------------------------------------------------
    // ComputeTotalXpFromLosers : somme des gainXp des perdants
    private int ComputeTotalXpFromLosers(List<GameObject> losers)
    {
        float sum = 0f;
        if (losers != null)
        {
            for (int i = 0; i < losers.Count; i++)
            {
                var entity = losers[i];
                if (!entity) continue;
                var info = entity.GetComponent<Entity_Info>();
                if (info != null) sum += info.gainXp;
            }
        }
        int total = Mathf.RoundToInt(sum);
        return Mathf.Max(0, total);
    }

    // ---------------------------------------------------------
    // FormatNumberGrouped : format "1 500" etc.
    private string FormatNumberGrouped(int value)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        nfi.NumberGroupSizes = new[] { 3 };
        return value.ToString("#,0", nfi);
    }

    // ---------------------------------------------------------
    // ComputeDropsForOneWinner : calcule les drops pour un gagnant
    private List<GameObject> ComputeDropsForOneWinner(List<GameObject> losers)
    {
        var drops = new List<GameObject>();
        if (losers == null || losers.Count == 0) return drops;

        for (int i = 0; i < losers.Count; i++)
        {
            var entity = losers[i];
            if (!entity) continue;

            var info = entity.GetComponent<Entity_Info>();
            if (info == null || info.listDropRessources == null) continue;

            foreach (var entry in info.listDropRessources)
            {
                if (entry == null) continue;

                GameObject prefab = entry.ressourcePrefab;
                float chance = Mathf.Clamp(entry.dropChance, 0f, 100f);

                if (!prefab) continue;

                if (RollChance(chance))
                    drops.Add(prefab);
            }
        }

        return drops;
    }

    // ---------------------------------------------------------
    // GiveItemsToInventory : ajoute chaque drop à l'inventaire
    private void GiveItemsToInventory(List<GameObject> dropPrefabs)
    {
        if (dropPrefabs == null || dropPrefabs.Count == 0) return;

        for (int i = 0; i < dropPrefabs.Count; i++)
        {
            var prefab = dropPrefabs[i];
            if (!prefab) continue;

            var ctrl = prefab.GetComponent<InventoryItemController>();
            Item item = ctrl != null ? ctrl.GetItem() : null;

            if (item != null)
            {
                InventoryUtil.AddItemToFirstEmpty(item);
            }
            else
            {
                Debug.LogWarning($"[End] Drop prefab sans Item lisible: {prefab.name}");
            }
        }
    }

    // ---------------------------------------------------------
    // RollChance : tirage pourcentage [0..100]
    private bool RollChance(float percent)
    {
        return Random.Range(0f, 100f) < percent;
    }

    // ---------------------------------------------------------
    // CreateLineForEntity : instancie une ligne Win/Lose
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

    // ---------------------------------------------------------
    // GetEntityDisplay : lit icône/nom depuis Entity_Info
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

    // ---------------------------------------------------------
    // ShowResultPopup : affiche la pop-up de résultat
    private void ShowResultPopup(bool win)
    {
        if (!resultPopupRoot) return;
        resultPopupRoot.SetActive(true);
        if (resultPopupText) resultPopupText.text = win ? winText : loseText;
    }

    // ---------------------------------------------------------
    // OnClick_CloseResultPopup : ferme la pop-up
    public void OnClick_CloseResultPopup()
    {
        if (resultPopupRoot) resultPopupRoot.SetActive(false);
    }

    // ---------------------------------------------------------
    // ReturnPlayerToExploration : renvoie un joueur en exploration
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

        ReactivateVisualsAndColliders(player);

        var stats = player.GetComponent<Entity_StatistiqueCombat>();
        if (stats != null) stats.isDead = false;

        player.SendMessage("OnCombatEnd", manager.lastCombatWinning, SendMessageOptions.DontRequireReceiver);
    }

    // ---------------------------------------------------------
    // RestorePlayerPosition : replace le joueur proprement (CharacterController off/on)
    private void RestorePlayerPosition(GameObject player, Vector3 savedPos)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = savedPos;
        if (cc != null) cc.enabled = true;
    }

    // ---------------------------------------------------------
    // RestorePlayerCameraConstraint : remet la source de ParentConstraint de la caméra du joueur
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

    // ---------------------------------------------------------
    // ReactivateVisualsAndColliders : remet ON tous les Renderers/Colliders du joueur
    private void ReactivateVisualsAndColliders(GameObject player)
    {
        if (!player) return;

        var renderers = player.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i]) continue;
            renderers[i].enabled = true;
        }

        var colliders = player.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i]) continue;
            colliders[i].enabled = true;
        }
    }

    // ---------------------------------------------------------
    // DestroyAllChildren : détruit les enfants d'un transform
    private void DestroyAllChildren(Transform root)
    {
        var toDestroy = new List<GameObject>();
        foreach (Transform child in root)
            if (child != null) toDestroy.Add(child.gameObject);

#if UNITY_EDITOR
        bool immediate = !Application.isPlaying;
#endif
        for (int i = 0; i < toDestroy.Count; i++)
        {
            var go = toDestroy[i];
            if (!go) continue;
#if UNITY_EDITOR
            if (immediate) DestroyImmediate(go);
            else
#endif
                Destroy(go);
        }
    }

    // ---------------------------------------------------------
    // ClearContainer : vide un container UI
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
