using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Combat/Phase - Enter Setup Combat (Local)")]
public class Phase_EnterSetupCombat : MonoBehaviour
{
    [Header("Carte utilisée pour ce combat")]
    public Data_FightMap combatMap;

    [Header("Équipes de combat")]
    public List<GameObject> greenTeam = new(); // Joueurs (team=0)
    public List<GameObject> redTeam = new();   // Monstres (team=1)
    public List<GameObject> AllFighters = new();

    [Header("Parents hiérarchiques (Drag & Drop ou auto-tag)")]
    public Transform teamRedParent;    // parent pour l'équipe Rouge
    public Transform teamGreenParent;  // parent pour l'équipe Verte

    [Tooltip("Si actif, trouvera les parents manquants en cherchant des objets taggés.")]
    public bool autoFindParentsByTagIfNull = true;
    [Tooltip("Tag pour le parent Rouge")] public string tagTeamRed = "TeamRed";
    [Tooltip("Tag pour le parent Vert")] public string tagTeamGreen = "TeamGreen";

    [Header("Options de placement")]
    [Tooltip("Force le reparenting de tous les monstres et joueurs à l'Init, même s'ils ont déjà un parent.")]
    public bool forceReparentOnInit = true;

    // ========================================
    [Header("UI (Scène unique)")]
    [Tooltip("Canvas racine de l'UI d'exploration (ex: 'Exploration_UI').")]
    [SerializeField] private GameObject explorationUIRoot;
    [Tooltip("Canvas racine de l'UI de combat (ex: 'Combat_UI').")]
    [SerializeField] private GameObject combatUIRoot;

    [Tooltip("À l'ouverture de la scène, activer Exploration_UI et désactiver Combat_UI.")]
    [SerializeField] private bool setInitialUIOnAwake = true;
    // ========================================

    private Exploration_InfoGroupMonster currentGroup;
    private Combat_PhaseManager manager;
    private readonly List<GameObject> pendingPlayers = new();

    private void Awake()
    {
        if (setInitialUIOnAwake)
        {
            if (explorationUIRoot != null) explorationUIRoot.SetActive(true);
            if (combatUIRoot != null) combatUIRoot.SetActive(false);
        }
    }

    // Appelée par Combat_PhaseManager.StartPhase(CombatPhase.Enter)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        Debug.Log($"[Enter] Arena {manager.arenaIndex} – phase d'entrée (LOCAL).");

        // 0) Switch UI : Exploration → OFF, Combat → ON
        if (explorationUIRoot != null) explorationUIRoot.SetActive(false);
        if (combatUIRoot != null) combatUIRoot.SetActive(true);
        else Debug.LogWarning("[Enter] Combat_UI introuvable. Assigne 'combatUIRoot' ou renomme l'objet en 'Combat_UI'.");

        // 1) Parents
        EnsureParents();

        // 2) Monstres → team=1 + parent Rouge
        SpawnMonstersInScene_Local();

        // 3) Joueurs → team=0 (+ parent Vert seulement si on est en combat)
        ReparentAllGreenPlayers();

        // 4) Joueurs arrivés avant Init
        foreach (var p in pendingPlayers) AddPlayerToTeamVerte(p);
        pendingPlayers.Clear();

        // 5) Rebuild liste brute
        RebuildAllFighters();
        //Initialiser les "current" depuis les "base" (HP/PA/PM/PO, résistances, etc.)
        EnsureCombatStatsInitialized();

        // 6) [ORDER] Construire l'ordre d'initiative avec alternance 1/1 quand possible
        var ordered = BuildInitiativeOrderInterleaved(AllFighters);
        AllFighters.Clear();
        AllFighters.AddRange(ordered);

        // 7) [TIMELINE] Construire la timeline maintenant que AllFighters est ordonné
        var timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);
        if (timeline)
        {
            timeline.BuildFromManager(manager);
            timeline.SetNoActive(); // aucun “Actif” pendant la Préparation
        }

        // 8) Phase suivante
        Debug.Log("[Enter] Fin → passage à la phase Préparation.");
        manager.NextPhase();
    }

    private void EnsureParents()
    {
        if (teamRedParent == null && autoFindParentsByTagIfNull)
        {
            var go = GameObject.FindGameObjectWithTag(tagTeamRed);
            if (go) teamRedParent = go.transform;
        }
        if (teamGreenParent == null && autoFindParentsByTagIfNull)
        {
            var go = GameObject.FindGameObjectWithTag(tagTeamGreen);
            if (go) teamGreenParent = go.transform;
        }

        if (teamRedParent == null) Debug.LogError("[Enter] teamRedParent manquant (Drag&Drop ou tag).", this);
        if (teamGreenParent == null) Debug.LogError("[Enter] teamGreenParent manquant (Drag&Drop ou tag).", this);
    }

    // Instancie OU réorganise les monstres localement
    private void SpawnMonstersInScene_Local()
    {
        if (teamRedParent == null) return;

        var monstersSpawned = new List<GameObject>();

        foreach (GameObject entry in redTeam)
        {
            if (!entry) continue;

            GameObject monsterInstance;
            bool isPrefab = !entry.scene.IsValid() || entry.scene == default(Scene);

            if (isPrefab)
            {
                monsterInstance = Instantiate(entry, teamRedParent.position, Quaternion.identity);
                monsterInstance.name = entry.name;
                Debug.Log($"[Enter] Monstre instancié depuis prefab : {monsterInstance.name}");
            }
            else
            {
                monsterInstance = entry;
                Debug.Log($"[Enter] Monstre déjà en scène : {monsterInstance.name}");
            }

            // Équipe = Rouge (1), état combat
            if (monsterInstance.TryGetComponent(out Entity_StatistiqueCombat mStats))
            {
                mStats.team = 1;
                mStats.isFight = true;

                // [FR] Sécurité d'init stats si besoin (HP/PA/PM/PO, etc.)
                if (mStats.baseHP > 0 && mStats.currentHP <= 0)
                    mStats.InitStatsFromBase();
            }

            // Parent rouge
            if (forceReparentOnInit || monsterInstance.transform.parent != teamRedParent)
                monsterInstance.transform.SetParent(teamRedParent, true);

            // ─────────────────────────────────────────────────────────
            // INJECTION : fournit Phase & Grid aux composants runtime des monstres
            // [FR] Monster_CombatController
            if (monsterInstance.TryGetComponent(out Monster_CombatController ai))
            {
                ai.phaseManager = manager;                 // [FR] Réf explicite (pas d'auto-find)
                ai.tileGrid = manager != null ? manager.tileGrid : null;
            }

            // [FR] Entity_SkillCaster (nouveau : injection identique)
            if (monsterInstance.TryGetComponent(out Entity_SkillCaster caster))
            {
                caster.phaseManager = manager;
                caster.tileGrid = manager != null ? manager.tileGrid : null;
            }
            // ─────────────────────────────────────────────────────────

            monstersSpawned.Add(monsterInstance);
        }

        redTeam = monstersSpawned;
    }


    private void ReparentAllGreenPlayers()
    {
        if (teamGreenParent == null) return;

        foreach (var player in greenTeam)
        {
            if (!player) continue;

            // Forçage équipe = Verte (0)
            if (player.TryGetComponent(out Entity_StatistiqueCombat pStats))
            {
                pStats.team = 0;
                pStats.isFight = true;
            }

            // Reparent uniquement si on est effectivement en combat
            if (manager != null && manager.isInCombat)
            {
                if (forceReparentOnInit || player.transform.parent != teamGreenParent)
                    player.transform.SetParent(teamGreenParent, true);
            }
        }
    }

    private void RebuildAllFighters()
    {
        AllFighters.Clear();
        AllFighters.AddRange(redTeam);
        AllFighters.AddRange(greenTeam);
    }

    public void SetCombatData(List<GameObject> newMonsters, Data_FightMap newMap, Exploration_InfoGroupMonster group)
    {
        redTeam = newMonsters ?? new List<GameObject>();
        combatMap = newMap;
        currentGroup = group;
        RebuildAllFighters();
        Debug.Log($"[Enter] Données OK – Monstres: {redTeam.Count} | Map: {(combatMap ? combatMap.name : "null")}");
    }

    public void AddPlayerToTeamVerte(GameObject player)
    {
        if (!player) return;

        if (manager == null)
        {
            Debug.LogWarning("[Enter] Manager null → joueur mis en attente.");
            if (!pendingPlayers.Contains(player)) pendingPlayers.Add(player);
            return;
        }

        if (!greenTeam.Contains(player))
        {
            greenTeam.Add(player);
            AllFighters.Add(player);

            if (player.TryGetComponent(out Entity_StatistiqueCombat stats))
            {
                stats.team = 0;
                stats.isFight = true;

                // ✅ S'assure que les "current" ne restent pas à 0 par défaut (HP/PA/PM/PO, résistances, etc.)
                if (stats.baseHP > 0 && stats.currentHP <= 0)
                    stats.InitStatsFromBase();
            }

            // ✅ Reparent seulement si on est en combat
            if (manager.isInCombat && teamGreenParent)
                player.transform.SetParent(teamGreenParent, true);

            if (manager.phasePrepa && manager.phasePrepa.isActiveAndEnabled)
                manager.phasePrepa.PlaceEntity(player);
            else
                Debug.Log("[Enter] Phase Prépa non active : placement différé.");
        }
        else
        {
            Debug.Log("[Enter] Joueur déjà dans l'équipe verte : " + player.name);
        }
    }

    public void SetMonsterState(MonsterState newState)
    {
        if (currentGroup != null) currentGroup.SetState(newState);
        else Debug.LogWarning("[Enter] Aucun groupe de monstres référencé pour SetMonsterState.");
    }

    // ============ [ORDER] Calcul ordre initiative + alternance 1/1 ============
    private List<GameObject> BuildInitiativeOrderInterleaved(List<GameObject> source)
    {
        var greens = new List<GameObject>();
        var reds = new List<GameObject>();

        // Split + filtre
        foreach (var e in source)
        {
            if (!e) continue;
            if (!e.TryGetComponent(out Entity_StatistiqueCombat s)) continue;
            if (s.team == 0) greens.Add(e);
            else reds.Add(e);
        }

        // Tri décroissant baseInitiative
        greens.Sort((a, b) =>
        {
            var sa = a.GetComponent<Entity_StatistiqueCombat>();
            var sb = b.GetComponent<Entity_StatistiqueCombat>();
            int ia = sa ? sa.baseInitiative : 0;
            int ib = sb ? sb.baseInitiative : 0;
            int cmp = ib.CompareTo(ia); // desc
            return (cmp != 0) ? cmp : string.Compare(a.name, b.name, System.StringComparison.Ordinal);
        });

        reds.Sort((a, b) =>
        {
            var sa = a.GetComponent<Entity_StatistiqueCombat>();
            var sb = b.GetComponent<Entity_StatistiqueCombat>();
            int ia = sa ? sa.baseInitiative : 0;
            int ib = sb ? sb.baseInitiative : 0;
            int cmp = ib.CompareTo(ia); // desc
            return (cmp != 0) ? cmp : string.Compare(a.name, b.name, System.StringComparison.Ordinal);
        });

        // Qui commence ? meilleur top ; égalité → Verte
        int topGreen = greens.Count > 0 ? (greens[0].GetComponent<Entity_StatistiqueCombat>()?.baseInitiative ?? 0) : -1;
        int topRed = reds.Count > 0 ? (reds[0].GetComponent<Entity_StatistiqueCombat>()?.baseInitiative ?? 0) : -1;
        int currentTeam = (topRed > topGreen) ? 1 : 0; // 0=Verte, 1=Rouge

        // Merge 1/1 tant que possible
        var order = new List<GameObject>(greens.Count + reds.Count);
        int gi = 0, ri = 0;
        while (gi < greens.Count || ri < reds.Count)
        {
            if (currentTeam == 0) // Verte
            {
                if (gi < greens.Count) { order.Add(greens[gi++]); currentTeam = 1; }
                else if (ri < reds.Count) { order.Add(reds[ri++]); }
            }
            else // Rouge
            {
                if (ri < reds.Count) { order.Add(reds[ri++]); currentTeam = 0; }
                else if (gi < greens.Count) { order.Add(greens[gi++]); }
            }
        }

#if UNITY_EDITOR
        System.Text.StringBuilder sb = new System.Text.StringBuilder("[Order] Timeline: ");
        for (int i = 0; i < order.Count; i++)
        {
            var s = order[i].GetComponent<Entity_StatistiqueCombat>();
            sb.Append($"{order[i].name}(T{s?.team}, Ini={s?.baseInitiative})");
            if (i < order.Count - 1) sb.Append(" -> ");
        }
        Debug.Log(sb.ToString());
#endif

        return order;
    }

    private void EnsureCombatStatsInitialized()
    {
        if (AllFighters == null) return;

        foreach (var go in AllFighters)
        {
            if (!go) continue;
            if (!go.TryGetComponent(out Entity_StatistiqueCombat s)) continue;

            // Heuristique : si currentHP est 0 alors que baseHP > 0, on considère non initialisé.
            // (évite d'écraser une entité déjà entamée)
            if (s.baseHP > 0 && s.currentHP <= 0)
            {
                s.InitStatsFromBase();
            }
        }
    }
}
