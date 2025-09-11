using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase_PreparationPlacementCombat : MonoBehaviour
{
    [Header("Paramètres de génération de la grille")]
    public GameObject tilePrefab;        // Prefab d'une tuile (doit contenir SetupTile)
    public Transform gridCenter;         // Parent/centre de la grille
    public Vector2 tileSpacing = new Vector2(1, 1); // Espacement entre tuiles (x=colonnes, y=lignes)
    public Vector3 gridRotation;         // Rotation globale de la grille (Euler)

    [Header("Rotation par défaut au placement")]
    [SerializeField] private Vector3 rotationGreenTeamEuler;
    [SerializeField] private Vector3 rotationRedTeamEuler;

    [Header("Hauteur Y par équipe (placement + déplacements Préparation)")]
    [SerializeField] private float greenTeamY = 2f;   // Hauteur imposée aux joueurs (équipe Verte)
    [SerializeField] private float redTeamY = 0.5f;   // Hauteur imposée aux monstres (équipe Rouge)

    // Exposition en lecture seule pour la phase TourParTour
    public float GreenTeamY => greenTeamY; // Permet de relire la même valeur en TurnByTurn
    public float RedTeamY => redTeamY;   // Permet de relire la même valeur en TurnByTurn

    // Références injectées par le manager
    private Combat_PhaseManager manager;
    private Data_FightMap mapData;
    private TileGrid_Manager tileGrid;

    // Appelée par Combat_PhaseManager.StartPhase(Preparation)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        // Récupère les références nécessaires à la phase
        manager = phaseManager;
        mapData = manager != null ? manager.phaseEnter?.combatMap : null;
        tileGrid = manager != null ? manager.tileGrid : null;

        Debug.Log($"[Prépa] Lancement (Arena {manager?.arenaIndex})");

        // Valide les champs indispensables et stoppe si manquants
        if (tilePrefab == null) { Debug.LogError("[Prépa] tilePrefab manquant."); return; }
        if (gridCenter == null) { Debug.LogError("[Prépa] gridCenter manquant."); return; }
        if (mapData == null) { Debug.LogError("[Prépa] Data_FightMap manquant."); return; }
        if (tileGrid == null) { Debug.LogError("[Prépa] TileGrid_Manager manquant."); return; }

        // Bascule le joueur en mode Préparation
        if (manager.phaseEnter != null)
        {
            foreach (var player in manager.phaseEnter.greenTeam)
            {
                if (!player) continue;
                var sm = player.GetComponent<Player_ScriptManager>();
                if (sm) sm.SetPreparationCombat(); // Active le mode "préparation" sur le joueur
            }
        }

        GenerateGrid();             // Génère la grille et enregistre chaque tuile dans tileGrid
        PlaceAllEntities();         // Place joueurs et monstres sur des cases libres de leur couleur
        Debug_ShowEntityTileLinks();// Affichage debug des liens tuile ↔ entité

        // Démarre la vérification périodique du "tout le monde prêt"
        StartCoroutine(CheckAllFightersReady());
    }

    // Génère toutes les tuiles à partir des données de map
    private void GenerateGrid()
    {
        gridCenter.rotation = Quaternion.Euler(gridRotation);

        Vector3 offset = new Vector3(
            (mapData.width - 1) * tileSpacing.x / 2f,
            0f,
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

                Vector3 localPos = new Vector3(x * tileSpacing.x, 0f, y * tileSpacing.y) - offset;
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity;

                if (!tile.TryGetComponent(out SetupTile setup))
                {
                    Debug.LogError($"[Prépa] Le prefab de tuile ne contient pas SetupTile → {tile.name}. Ignorée.");
                    Destroy(tile);
                    continue;
                }

                setup.tileX = x;
                setup.tileY = y;
                setup.syncedName = tile.name;

                if (mapData.greenTeamPositions.Contains(pos))
                    setup.currentState = Tile_State.TeamGreen;
                else if (mapData.redTeamPositions.Contains(pos))
                    setup.currentState = Tile_State.TeamRed;
                else if (mapData.interactiveObjectPositions.Contains(pos))
                    setup.currentState = Tile_State.Obstacle;
                else
                    setup.currentState = Tile_State.None;

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

    // Place chaque entité (joueurs + monstres) sur une case libre de sa couleur
    private void PlaceAllEntities()
    {
        List<GameObject> fighters = manager.phaseEnter.AllFighters;
        for (int i = 0; i < fighters.Count; i++)
            PlaceEntity(fighters[i]);
    }

    // Calcule la position monde à utiliser pour une équipe donnée sur une tuile
    private Vector3 GetWorldPosForTeamOnTile(int team, Transform tileTransform)
    {
        Vector3 p = tileTransform.position; // base monde = position de la tuile
        p.y = (team == 0) ? greenTeamY : redTeamY; // hauteur absolue contrôlée par l'Inspector
        return p;
    }

    // Place une entité sur une case libre correspondant à sa team
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

        Vector3 newPosition = GetWorldPosForTeamOnTile(stats.team, tile.transform);
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

    private GameObject GetFreeTileForTeam(int team)
    {
        var tiles = tileGrid.GetAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if (tile == null) continue;
            if (!tileGrid.IsTileFree(tile)) continue;
            if (!tile.TryGetComponent(out SetupTile setup)) continue;

            if (team == 0 && setup.currentState == Tile_State.TeamGreen) return tile;
            if (team == 1 && setup.currentState == Tile_State.TeamRed) return tile;
        }
        return null;
    }

    public GameObject GetTileAtCoordinates(int x, int y) => tileGrid.GetTileAtCoordinates(x, y);
    public bool IsTileFree(GameObject tile) => tileGrid.IsTileFree(tile);

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

        Vector3 newPosition = GetWorldPosForTeamOnTile(0, tile.transform);
        Quaternion newRotation = Quaternion.Euler(rotationGreenTeamEuler);

        playerObj.transform.SetPositionAndRotation(newPosition, newRotation);
        tileGrid.RegisterEntity(playerObj, tile);

        Debug.Log($"[Prépa] {playerObj.name} → {tile.name}");
    }

    private IEnumerator CheckAllFightersReady()
    {
        Debug.Log("[Prépa] Vérification readiness…");
        var wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;

            var allFighters = manager.phaseEnter.AllFighters;
            bool allFightersReady = true;

            for (int i = 0; i < allFighters.Count; i++)
            {
                var entityObj = allFighters[i];
                if (entityObj == null) continue;

                if (!entityObj.TryGetComponent(out Entity_StatistiqueCombat stats))
                {
                    allFightersReady = false;
                    break;
                }

                if (!stats.isReady)
                {
                    allFightersReady = false;
                    break;
                }
            }

            if (allFightersReady)
            {
                Debug.Log("[Prépa] TOUS prêts → Passage TourParTour.");
                manager.StartPhase(CombatPhase.TurnByTurn);
                yield break;
            }
        }
    }

    public void Debug_ShowEntityTileLinks()
    {
        Debug.Log("-------------------------------------------------------------------");
        Debug.Log("--- [DEBUG] État des cases (via TileGrid_Manager) ---");

        var tiles = tileGrid.GetAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if (tile == null) continue;

            string tileName = tile.name;

            if (tile.TryGetComponent(out SetupTile local))
                tileName += $" ({local.tileX}, {local.tileY})";

            GameObject occupant = tileGrid.GetEntityOnTile(tile);
            string occupantName = occupant != null ? occupant.name : "VIDE";

            Debug.Log($" > {tileName} contient : {occupantName}");
        }

        Debug.Log("-------------------------------------------------------------------");
    }
}
