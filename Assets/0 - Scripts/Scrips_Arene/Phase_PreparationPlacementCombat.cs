using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Preparation Placement (Local)")]
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

    // Références injectées par le manager
    private Combat_PhaseManager manager;
    private Data_FightMap mapData;
    private TileGrid_Manager tileGrid;

    // Appelée par Combat_PhaseManager.StartPhase(Preparation)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        // Références nécessaires
        manager = phaseManager;
        mapData = manager != null ? manager.phaseEnter?.combatMap : null;
        tileGrid = manager != null ? manager.tileGrid : null;

        Debug.Log($"[Prépa] Lancement (Arena {manager?.arenaIndex})");

        // Validation minimale et arrêt immédiat si manquant
        if (tilePrefab == null) { Debug.LogError("[Prépa] tilePrefab manquant."); return; }
        if (gridCenter == null) { Debug.LogError("[Prépa] gridCenter manquant."); return; }
        if (mapData == null) { Debug.LogError("[Prépa] Data_FightMap manquant."); return; }
        if (tileGrid == null) { Debug.LogError("[Prépa] TileGrid_Manager manquant."); return; }

        // Passage en mode Préparation pour l'équipe verte (UI/contrôles côté joueur)
        if (manager.phaseEnter != null)
        {
            foreach (var player in manager.phaseEnter.greenTeam)
            {
                if (!player) continue;
                var sm = player.GetComponent<Player_ScriptManager>();
                if (sm) sm.SetPreparationCombat(); // Active le mode "préparation" sur le joueur
            }
        }

        GenerateGrid();        // Génère la grille et enregistre chaque tuile dans tileGrid
        PlaceAllEntities();    // Place joueurs et monstres sur des cases libres de leur couleur
        Debug_ShowEntityTileLinks(); // Aide au debug

        // Démarre la vérification périodique du "tout le monde prêt"
        StartCoroutine(CheckAllFightersReady());
    }

    // Génération complète de la grille à partir de mapData
    private void GenerateGrid()
    {
        // Applique la rotation globale au conteneur de la grille
        gridCenter.rotation = Quaternion.Euler(gridRotation);

        // Calcule l'offset pour centrer la grille autour de gridCenter
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

                // Instancie la tuile
                GameObject tile = Instantiate(tilePrefab);
                tile.name = $"Case_{x}_{y}";
                tile.transform.SetParent(gridCenter, false);

                // Position/rotation locale
                Vector3 localPos = new Vector3(x * tileSpacing.x, 0f, y * tileSpacing.y) - offset;
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity;

                // Le prefab doit déjà avoir SetupTile (pas d'ajout automatique)
                if (!tile.TryGetComponent(out SetupTile setup))
                {
                    Debug.LogError($"[Prépa] Le prefab de tuile ne contient pas SetupTile → {tile.name}. Ignorée.");
                    Destroy(tile);
                    continue;
                }

                // Remplit les infos de la tuile
                setup.tileX = x;
                setup.tileY = y;
                setup.syncedName = tile.name;

                // État initial en fonction des zones de la map
                if (mapData.greenTeamPositions.Contains(pos))
                    setup.currentState = Tile_State.TeamGreen;
                else if (mapData.redTeamPositions.Contains(pos))
                    setup.currentState = Tile_State.TeamRed;
                else if (mapData.interactiveObjectPositions.Contains(pos))
                    setup.currentState = Tile_State.Obstacle;
                else
                    setup.currentState = Tile_State.None;

                // Enregistrement dans la grille
                tileGrid.RegisterTile(tile);
            }
        }

        // Log récapitulatif des zones
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

    // Place une entité sur une case libre correspondant à sa team
    public void PlaceEntity(GameObject entityObj)
    {
        if (entityObj == null) return;

        if (!entityObj.TryGetComponent(out Entity_StatistiqueCombat stats))
        {
            Debug.LogWarning("[Prépa] Impossible de placer l’entité (pas de Entity_StatistiqueCombat).", entityObj);
            return;
        }

        // Normalise l'équipe si besoin (0=Vert, 1=Rouge)
        if (stats.team != 0 && stats.team != 1)
        {
            Debug.LogWarning($"[Prépa] {entityObj.name} a une team invalide ({stats.team}). Forçage Rouge.");
            stats.team = 1;
        }

        // Récupère une case libre pour l'équipe
        GameObject tile = GetFreeTileForTeam(stats.team);
        if (tile == null)
        {
            Debug.LogError($"[Prépa] Aucune case libre trouvée pour {(stats.team == 0 ? "VERT" : "ROUGE")} !");
            return;
        }

        // Positionne et enregistre le lien entité ↔ tuile
        Vector3 newPosition = tile.transform.position + Vector3.up * 0.5f;
        Quaternion newRotation = (stats.team == 0)
            ? Quaternion.Euler(rotationGreenTeamEuler)
            : Quaternion.Euler(rotationRedTeamEuler);

        entityObj.transform.SetPositionAndRotation(newPosition, newRotation);
        tileGrid.RegisterEntity(entityObj, tile);

        // Les monstres sont automatiquement "prêts" en phase de préparation
        if (stats.team == 1 && !stats.isReady)
        {
            stats.isReady = true;
            Debug.Log($"[Prépa] {entityObj.name} (Monstre) → isReady = true");
        }
    }

    // Retourne une case libre correspondant à l'équipe demandée (0=Vert, 1=Rouge)
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

    // Accès utiles pour d'autres scripts
    public GameObject GetTileAtCoordinates(int x, int y) => tileGrid.GetTileAtCoordinates(x, y);
    public bool IsTileFree(GameObject tile) => tileGrid.IsTileFree(tile);

    // Déplacement libre d’un joueur VERT vers une case verte libre (pendant la Préparation)
    public void TryMoveEntityToTile(GameObject playerObj, int x, int y)
    {
        if (!isActiveAndEnabled) return;
        if (playerObj == null) return;

        var tile = tileGrid.GetTileAtCoordinates(x, y);
        if (tile == null) return;
        if (!tile.TryGetComponent(out SetupTile setup)) return;

        // Uniquement sur zone verte, case libre, et entité de l'équipe verte
        if (setup.currentState != Tile_State.TeamGreen) return;
        if (!tileGrid.IsTileFree(tile)) return;
        if (!playerObj.TryGetComponent(out Entity_StatistiqueCombat stats)) return;
        if (stats.team != 0) return;

        // Déplace et enregistre
        Vector3 newPosition = tile.transform.position + Vector3.up * 0.5f;
        Quaternion newRotation = Quaternion.Euler(rotationGreenTeamEuler);

        playerObj.transform.SetPositionAndRotation(newPosition, newRotation);
        tileGrid.RegisterEntity(playerObj, tile);

        Debug.Log($"[Prépa] {playerObj.name} → {tile.name}");
    }

    // Vérifie régulièrement si toutes les entités sont prêtes, puis passe à la phase TourParTour
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

    // Affichage debug : contenu de chaque case (utile pour vérifier les liens)
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
