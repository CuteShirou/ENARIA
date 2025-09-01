using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Phase_EnterSetupCombat : MonoBehaviour
{
    [Header("Carte utilisée pour ce combat")]
    public Data_FightMap combatMap;

    [Header("Équipes de combat")]
    public List<GameObject> greenTeam = new(); // Joueurs (team=0)
    public List<GameObject> redTeam = new();   // Monstres (team=1)
    public List<GameObject> AllFighters = new();

    [Header("Parents hiérarchiques (Drag & Drop obligatoire)")]
    public Transform teamRedParent;    // parent pour l'équipe Rouge
    public Transform teamGreenParent;  // parent pour l'équipe Verte

    [Header("Options de placement")]
    [Tooltip("Force le reparenting de tous les monstres et joueurs à l'Init, même s'ils ont déjà un parent.")]
    public bool forceReparentOnInit = true;

    [Header("UI (références à assigner)")]
    [SerializeField] private GameObject explorationUIRoot; // Canvas Exploration
    [SerializeField] private GameObject combatUIRoot;      // Canvas Combat
    [SerializeField] private bool setInitialUIOnAwake = true;

    [Header("Timeline (référence directe, pas d'auto-find)")]
    [SerializeField] private Timeline_CombatUI timelineUI;

    private Exploration_InfoGroupMonster currentGroup;
    private Combat_PhaseManager manager;
    private readonly List<GameObject> pendingPlayers = new();

    private void Awake()
    {
        // Active/désactive les Canvas selon l'état initial demandé
        if (setInitialUIOnAwake)
        {
            if (explorationUIRoot != null) explorationUIRoot.SetActive(true);
            if (combatUIRoot != null) combatUIRoot.SetActive(false);
        }
    }

    // Démarrage de la phase d'entrée (appelée par Combat_PhaseManager)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        Debug.Log($"[Enter] Arena {manager.arenaIndex} – phase d'entrée (LOCAL).");

        // Bascule d'UI : Exploration OFF, Combat ON
        if (explorationUIRoot != null) explorationUIRoot.SetActive(false);
        if (combatUIRoot != null) combatUIRoot.SetActive(true);
        else Debug.LogWarning("[Enter] 'combatUIRoot' non assigné.");

        // Vérifie que les parents hiérarchiques sont assignés
        ValidateParentsOrLog();

        // Prépare/instancie les monstres et les configure pour le combat
        SpawnMonstersInScene_Local();

        // Reparent les joueurs verts et force le mode combat
        ReparentAllGreenPlayers();

        // Ajoute les joueurs reçus avant l'Init
        foreach (var p in pendingPlayers) AddPlayerToTeamVerte(p);
        pendingPlayers.Clear();

        // Reconstruit la liste globale et initialise les stats si nécessaire
        RebuildAllFighters();
        EnsureCombatStatsInitialized();

        // Construit l'ordre d'initiative en alternant si possible
        var ordered = BuildInitiativeOrderInterleaved(AllFighters);
        AllFighters.Clear();
        AllFighters.AddRange(ordered);

        // Construit la timeline si la référence est fournie
        if (timelineUI != null)
        {
            timelineUI.BuildFromManager(manager);
            timelineUI.SetNoActive(); // aucun actif pendant la Préparation
        }

        // Enchaîne vers la phase de Préparation
        Debug.Log("[Enter] Fin → passage à la phase Préparation.");
        manager.NextPhase();
    }

    // Vérifie que teamRedParent et teamGreenParent sont présents
    private void ValidateParentsOrLog()
    {
        if (teamRedParent == null)
            Debug.LogError("[Enter] teamRedParent manquant. Assigne la référence dans l'Inspector.", this);

        if (teamGreenParent == null)
            Debug.LogError("[Enter] teamGreenParent manquant. Assigne la référence dans l'Inspector.", this);
    }

    // Instancie ou réorganise les monstres localement et injecte les références nécessaires
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

            // Configure l'équipe et le mode combat + init des stats si nécessaire
            if (monsterInstance.TryGetComponent(out Entity_StatistiqueCombat mStats))
            {
                mStats.team = 1;
                mStats.isFight = true;

                if (mStats.baseHP > 0 && mStats.currentHP <= 0)
                    mStats.InitStatsFromBase();
            }

            // Reparent sous le parent Rouge
            if (forceReparentOnInit || monsterInstance.transform.parent != teamRedParent)
                monsterInstance.transform.SetParent(teamRedParent, true);

            // Injection : fournit Phase & Grid aux scripts runtime
            if (monsterInstance.TryGetComponent(out Monster_CombatController ai))
            {
                ai.phaseManager = manager;
                ai.tileGrid = manager != null ? manager.tileGrid : null;
            }

            if (monsterInstance.TryGetComponent(out Entity_SkillCaster caster))
            {
                caster.phaseManager = manager;
                caster.tileGrid = manager != null ? manager.tileGrid : null;
            }

            monstersSpawned.Add(monsterInstance);
        }

        redTeam = monstersSpawned;
    }

    // Reparent les joueurs verts et force leur statut de combat
    private void ReparentAllGreenPlayers()
    {
        if (teamGreenParent == null) return;

        foreach (var player in greenTeam)
        {
            if (!player) continue;

            if (player.TryGetComponent(out Entity_StatistiqueCombat pStats))
            {
                pStats.team = 0;
                pStats.isFight = true;
            }

            if (manager != null && manager.isInCombat)
            {
                if (forceReparentOnInit || player.transform.parent != teamGreenParent)
                    player.transform.SetParent(teamGreenParent, true);
            }
        }
    }

    // Reconstruit la liste globale AllFighters
    private void RebuildAllFighters()
    {
        AllFighters.Clear();
        AllFighters.AddRange(redTeam);
        AllFighters.AddRange(greenTeam);
    }

    // Renseigne les données de combat (monstres, map, groupe source)
    public void SetCombatData(List<GameObject> newMonsters, Data_FightMap newMap, Exploration_InfoGroupMonster group)
    {
        redTeam = newMonsters ?? new List<GameObject>();
        combatMap = newMap;
        currentGroup = group;
        RebuildAllFighters();
        Debug.Log($"[Enter] Données OK – Monstres: {redTeam.Count} | Map: {(combatMap ? combatMap.name : "null")}");
    }

    // Ajoute un joueur côté Vert et tente son placement si la Préparation est active
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

                if (stats.baseHP > 0 && stats.currentHP <= 0)
                    stats.InitStatsFromBase();
            }

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

    // Met à jour l'état du groupe de monstres d'exploration
    public void SetMonsterState(MonsterState newState)
    {
        if (currentGroup != null) currentGroup.SetState(newState);
        else Debug.LogWarning("[Enter] Aucun groupe de monstres référencé pour SetMonsterState.");
    }

    // Construit un ordre d'initiative alterné (Vert/Rouge) à partir des initiatives
    private List<GameObject> BuildInitiativeOrderInterleaved(List<GameObject> source)
    {
        var greens = new List<GameObject>();
        var reds = new List<GameObject>();

        foreach (var e in source)
        {
            if (!e) continue;
            if (!e.TryGetComponent(out Entity_StatistiqueCombat s)) continue;
            if (s.team == 0) greens.Add(e);
            else reds.Add(e);
        }

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

        int topGreen = greens.Count > 0 ? (greens[0].GetComponent<Entity_StatistiqueCombat>()?.baseInitiative ?? 0) : -1;
        int topRed = reds.Count > 0 ? (reds[0].GetComponent<Entity_StatistiqueCombat>()?.baseInitiative ?? 0) : -1;
        int currentTeam = (topRed > topGreen) ? 1 : 0; // 0=Verte, 1=Rouge

        var order = new List<GameObject>(greens.Count + reds.Count);
        int gi = 0, ri = 0;
        while (gi < greens.Count || ri < reds.Count)
        {
            if (currentTeam == 0)
            {
                if (gi < greens.Count) { order.Add(greens[gi++]); currentTeam = 1; }
                else if (ri < reds.Count) { order.Add(reds[ri++]); }
            }
            else
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

    // Initialise les stats de combat si l'entité n'a pas encore été préparée
    private void EnsureCombatStatsInitialized()
    {
        if (AllFighters == null) return;

        foreach (var go in AllFighters)
        {
            if (!go) continue;
            if (!go.TryGetComponent(out Entity_StatistiqueCombat s)) continue;

            if (s.baseHP > 0 && s.currentHP <= 0)
            {
                s.InitStatsFromBase();
            }
        }
    }
}
