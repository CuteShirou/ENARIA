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
    public List<GameObject> redTeam = new(); // Monstres (team=1)
    public List<GameObject> AllFighters = new();

    [Header("Parents hiérarchiques (Drag & Drop ou auto-tag)")]
    public Transform teamRedParent;    // parent pour l'équipe Rouge
    public Transform teamGreenParent;  // parent pour l'équipe Verte

    [Tooltip("Si actif, trouvera les parents manquants en cherchant des objets taggés.")]
    public bool autoFindParentsByTagIfNull = true;
    [Tooltip("Tag pour le parent Rouge")]
    public string tagTeamRed = "TeamRed";
    [Tooltip("Tag pour le parent Vert")]
    public string tagTeamGreen = "TeamGreen";

    [Header("Options de placement")]
    [Tooltip("Force le reparenting de tous les monstres et joueurs à l'Init, même s'ils ont déjà un parent.")]
    public bool forceReparentOnInit = true;

    // ==================== UI (scène unique) ====================
    [Header("UI (Scène unique)")]
    [Tooltip("Canvas racine de l'UI d'exploration (ex: 'Exploration_UI').")]
    [SerializeField] private GameObject explorationUIRoot;
    [Tooltip("Canvas racine de l'UI de combat (ex: 'Combat_UI').")]
    [SerializeField] private GameObject combatUIRoot;

    [Tooltip("Tenter de retrouver automatiquement par nom si non assigné.")]
    [SerializeField] private bool autoFindUIsIfNull = true;

    [SerializeField] private string explorationUIObjectName = "Exploration_UI";
    [SerializeField] private string combatUIObjectName = "Combat_UI";

    [Tooltip("À l'ouverture de la scène, activer Exploration_UI et désactiver Combat_UI.")]
    [SerializeField] private bool setInitialUIOnAwake = true;
    // ===========================================================

    private Exploration_InfoGroupMonster currentGroup;
    private Combat_PhaseManager manager;
    private readonly List<GameObject> pendingPlayers = new();

    private void Awake()
    {
        if (setInitialUIOnAwake)
        {
            EnsureUIReferences();
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
        EnsureUIReferences();
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

        // 5) Rebuild liste
        RebuildAllFighters();

        // 6) Phase suivante
        Debug.Log("[Enter] Fin → passage à la phase Préparation.");
        manager.NextPhase();
    }

    private void EnsureUIReferences()
    {
        if (autoFindUIsIfNull)
        {
            if (explorationUIRoot == null)
            {
                var go = GameObject.Find(explorationUIObjectName);
                if (go != null) explorationUIRoot = go;
            }
            if (combatUIRoot == null)
            {
                var go = GameObject.Find(combatUIObjectName);
                if (go != null) combatUIRoot = go;
            }
        }
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

    // Instancie OU réorganise les monstres localement (aucun réseau)
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

            // Forçage équipe = Rouge (1)
            if (monsterInstance.TryGetComponent(out Entity_StatistiqueCombat mStats))
                mStats.team = 1;

            if (forceReparentOnInit || monsterInstance.transform.parent != teamRedParent)
                monsterInstance.transform.SetParent(teamRedParent, true);

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

            // ✅ Reparent uniquement si on est effectivement en combat
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
            if (!pendingPlayers.Contains(player))
                pendingPlayers.Add(player);
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
}
