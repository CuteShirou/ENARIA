using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
// Phase locale (sans Mirror) pour la préparation/placement
[AddComponentMenu("Combat/Phase - Preparation Placement (Local)")]
public class Phase_PreparationPlacementCombat : MonoBehaviour
{
    [Header("Paramètres de génération de la grille")]
    public GameObject tilePrefab;
    public Transform gridCenter;
    public Vector2 tileSpacing = new Vector2(1, 1);
    public Vector3 gridRotation;

    [Header("Rotation par défaut au placement")]
    [SerializeField] private Vector3 rotationGreenTeamEuler;
    [SerializeField] private Vector3 rotationRedTeamEuler;

    // Références injectées par le manager
    private Combat_PhaseManager manager;
    private Data_FightMap mapData;
    private TileGrid_Manager tileGrid;

    // -------------------------------------------------------------
    // Appelée par Combat_PhaseManager.StartPhase(Preparation)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;
        mapData = manager.phaseEnter.combatMap;
        tileGrid = manager.tileGrid;

        Debug.Log($"[Prépa] Lancement (Arena {manager.arenaIndex})");

        if (!ValidateReferences()) return;

        // ➜ Bascule en mode Préparation pour tous les joueurs (équipe verte)
        if (manager.phaseEnter != null)
        {
            foreach (var player in manager.phaseEnter.greenTeam)
            {
                if (!player) continue;
                var sm = player.GetComponent<Player_ScriptManager>();
                if (sm) sm.SetPreparationCombat();
            }
        }

        GenerateGrid();
        PlaceAllEntities();
        Debug_ShowEntityTileLinks();

        // Vérification périodique du "tout le monde prêt"
        StartCoroutine(CheckAllFightersReady());
    }

    // -------------------------------------------------------------
    private bool ValidateReferences()
    {
        bool anyNull = false;
        anyNull |= LogIfNull(tilePrefab, nameof(tilePrefab));
        anyNull |= LogIfNull(gridCenter, nameof(gridCenter));
        anyNull |= LogIfNull(mapData, nameof(mapData));
        anyNull |= LogIfNull(tileGrid, nameof(tileGrid));

        if (anyNull)
        {
            Debug.LogError("[Prépa] Références manquantes → arrêt de la phase.", this);
            return false;
        }
        return true;
    }

    private bool LogIfNull(Object obj, string label)
    {
        if (obj == null)
        {
            Debug.LogError($"[Prépa] Référence manquante : {label}", this);
            return true;
        }
        return false;
    }

    // -------------------------------------------------------------
    private void GenerateGrid()
    {
        // Rotation globale de la grille
        gridCenter.rotation = Quaternion.Euler(gridRotation);

        // Offset pour centrer la grille
        Vector3 offset = new Vector3(
            (mapData.width - 1) * tileSpacing.x / 2f,
            0,
            (mapData.height - 1) * tileSpacing.y / 2f
        );

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameObject tile = Instantiate(tilePrefab);

                tile.name = $"Case_{x}_{y}";
                tile.transform.SetParent(gridCenter, false);

                Vector3 localPos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y) - offset;
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity;

                var setup = tile.GetComponent<SetupTile>();
                if (setup == null) setup = tile.AddComponent<SetupTile>();

                setup.tileX = x;
                setup.tileY = y;
                setup.syncedName = tile.name;
                setup.currentState =
                    mapData.greenTeamPositions.Contains(pos) ? Tile_State.TeamGreen :
                    mapData.redTeamPositions.Contains(pos) ? Tile_State.TeamRed :
                    mapData.interactiveObjectPositions.Contains(pos) ? Tile_State.Obstacle :
                    Tile_State.None;

                tileGrid.RegisterTile(tile);
            }
        }

        int greens = 0, reds = 0;
        foreach (var t in tileGrid.GetAllTiles())
        {
            if (t != null && t.TryGetComponent(out SetupTile s))
            {
                if (s.currentState == Tile_State.TeamGreen) greens++;
                else if (s.currentState == Tile_State.TeamRed) reds++;
            }
        }
        Debug.Log($"[Prépa] Grille OK. Tuiles Vertes: {greens} | Tuiles Rouges: {reds}");
    }

    // -------------------------------------------------------------
    private void PlaceAllEntities()
    {
        List<GameObject> fighters = manager.phaseEnter.AllFighters;
        foreach (GameObject entityObj in fighters)
            PlaceEntity(entityObj);
    }

    // -------------------------------------------------------------
    public void PlaceEntity(GameObject entityObj)
    {
        if (entityObj == null) return;

        if (!entityObj.TryGetComponent(out Entity_StatistiqueCombat stats))
        {
            Debug.LogWarning("[Prépa] Impossible de placer l’entité (pas de Entity_StatistiqueCombat).", entityObj);
            return;
        }

        if (stats.team != 0 && stats.team != 1)
        {
            Debug.LogWarning($"[Prépa] {entityObj.name} a une team invalide ({stats.team}). Forçage Rouge.");
            stats.team = 1;
        }

        GameObject tile = GetFreeTileForTeam(stats.team);
        if (tile == null)
        {
            Debug.LogError($"[Prépa] Aucune case libre trouvée pour {(stats.team == 0 ? "VERT" : "ROUGE")} !");
            return;
        }

        Vector3 newPosition = tile.transform.position + Vector3.up * 0.5f;
        Quaternion newRotation = (stats.team == 0)
            ? Quaternion.Euler(rotationGreenTeamEuler)
            : Quaternion.Euler(rotationRedTeamEuler);

        entityObj.transform.SetPositionAndRotation(newPosition, newRotation);
        tileGrid.RegisterEntity(entityObj, tile);

        if (stats.team == 1 && !stats.isReady)
        {
            stats.isReady = true;
            Debug.Log($"[Prépa] {entityObj.name} (Monstre) → isReady = true");
        }
    }

    // -------------------------------------------------------------
    private GameObject GetFreeTileForTeam(int team) // 0=Vert, 1=Rouge
    {
        foreach (GameObject tile in tileGrid.GetAllTiles())
        {
            if (!tileGrid.IsTileFree(tile)) continue;
            if (!tile.TryGetComponent(out SetupTile setup)) continue;

            if (team == 0 && setup.currentState == Tile_State.TeamGreen) return tile;
            if (team == 1 && setup.currentState == Tile_State.TeamRed) return tile;
        }
        return null;
    }

    public GameObject GetTileAtCoordinates(int x, int y) => tileGrid.GetTileAtCoordinates(x, y);
    public bool IsTileFree(GameObject tile) => tileGrid.IsTileFree(tile);

    // -------------------------------------------------------------
    public void TryMoveEntityToTile(GameObject playerObj, int x, int y)
    {
        if (!isActiveAndEnabled) return;
        if (playerObj == null) return;

        var tile = tileGrid.GetTileAtCoordinates(x, y);
        if (tile == null) return;
        if (!tile.TryGetComponent(out SetupTile setup)) return;

        if (setup.currentState != Tile_State.TeamGreen) return;
        if (!tileGrid.IsTileFree(tile)) return;

        if (!playerObj.TryGetComponent(out Entity_StatistiqueCombat stats)) return;
        if (stats.team != 0) return;

        Vector3 newPosition = tile.transform.position + Vector3.up * 0.5f;
        Quaternion newRotation = Quaternion.Euler(rotationGreenTeamEuler);

        playerObj.transform.SetPositionAndRotation(newPosition, newRotation);
        tileGrid.RegisterEntity(playerObj, tile);

        Debug.Log($"[Prépa] {playerObj.name} → {tile.name}");
    }

    // -------------------------------------------------------------
    private IEnumerator CheckAllFightersReady()
    {
        Debug.Log("[Prépa] Vérification readiness…");
        var wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;

            List<GameObject> allFighters = manager.phaseEnter.AllFighters;
            bool allFightersReady = true;

            foreach (GameObject entityObj in allFighters)
            {
                if (entityObj == null) continue;
                if (!entityObj.TryGetComponent(out Entity_StatistiqueCombat stats)) { allFightersReady = false; break; }
                if (!stats.isReady) { allFightersReady = false; break; }
            }

            if (allFightersReady)
            {
                Debug.Log("[Prépa] TOUS prêts → Passage TourParTour.");
                manager.StartPhase(CombatPhase.TurnByTurn);
                yield break;
            }
        }
    }

    // -------------------------------------------------------------
    public void Debug_ShowEntityTileLinks()
    {
        Debug.Log($"-------------------------------------------------------------------");
        Debug.Log($"--- [DEBUG] État des cases (via TileGrid_Manager) ---");

        foreach (GameObject tile in tileGrid.GetAllTiles())
        {
            string tileName = tile.name;

            if (tile.TryGetComponent(out SetupTile local))
                tileName += $" ({local.tileX}, {local.tileY})";

            GameObject occupant = tileGrid.GetEntityOnTile(tile);
            string occupantName = occupant != null ? occupant.name : "VIDE";

            Debug.Log($" > {tileName} contient : {occupantName}");
        }

        Debug.Log($"-------------------------------------------------------------------");
    }
}
