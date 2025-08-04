using System.Collections.Generic;
using UnityEngine;
using Mirror;

//--------------------------------------------------------------
public class Phase_PreparationPlacementCombat : NetworkBehaviour
{
    [Header("Paramètres de génération de la grille")]
    public GameObject tilePrefab;
    public Transform gridCenter;
    public Vector2 tileSpacing = new Vector2(1, 1);
    public Vector3 gridRotation;

    [Header("Rotation par défaut au placement")]
    [SerializeField] private Vector3 rotationGreenTeamEuler;
    [SerializeField] private Vector3 rotationRedTeamEuler;

    private Combat_PhaseManager manager;
    private Data_FightMap mapData;

    private Dictionary<GameObject, GameObject> entityToTile = new();
    private Dictionary<GameObject, GameObject> tileToEntity = new();
    private List<GameObject> allTiles = new();

    //--------------------------------------------------------------
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;
        mapData = manager.phaseEnter.combatMap;

        Debug.Log($"[Phase_Preparation] Lancement sur l'arène {manager.arenaIndex}");

        if (isServer)
        {
            Debug.Log("[Phase_Preparation] Génération de la grille côté serveur...");
            GenerateGrid();

            Debug.Log("[Phase_Preparation] Placement automatique des entités...");
            PlaceAllEntities();

            Debug_ShowEntityTileLinks();
        }
    }

    //--------------------------------------------------------------
    [Server]
    private void GenerateGrid()
    {
        gridCenter.rotation = Quaternion.Euler(gridRotation);

        Vector3 offset = new Vector3(
            (mapData.width - 1) * tileSpacing.x / 2f,
            0,
            (mapData.height - 1) * tileSpacing.y / 2f
        );

        NetworkIdentity parentNetId = gridCenter.GetComponent<NetworkIdentity>();
        if (parentNetId == null)
        {
            Debug.LogError("[Phase_Preparation] ERREUR : gridCenter n'a pas de NetworkIdentity !");
            return;
        }

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameObject tile = Instantiate(tilePrefab);

                tile.name = $"Case_{x}_{y}";
                tile.transform.SetParent(gridCenter);

                Vector3 localPos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y) - offset;
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity;

                Setup_NetworkTile tileNet = tile.GetComponent<Setup_NetworkTile>();

                if (tileNet != null)
                {
                    tileNet.SetTileCoordinates(x, y);
                    tileNet.syncedName = tile.name;

                    if (mapData.greenTeamPositions.Contains(pos))
                        tileNet.currentState = TileState.TeamGreen;
                    else if (mapData.redTeamPositions.Contains(pos))
                        tileNet.currentState = TileState.TeamRed;
                    else if (mapData.interactiveObjectPositions.Contains(pos))
                        tileNet.currentState = TileState.Obstacle;
                    else
                        tileNet.currentState = TileState.None;

                    tileNet.parentNetId = parentNetId.netId;
                }

                NetworkServer.Spawn(tile);
                allTiles.Add(tile);
                tileToEntity[tile] = null;
            }
        }

        Debug.Log("[Phase_Preparation] Grille complète générée et centrée.");
    }

    //--------------------------------------------------------------
    [Server]
    private void PlaceAllEntities()
    {
        List<GameObject> fighters = manager.phaseEnter.AllFighters;

        foreach (GameObject entityObj in fighters)
        {
            PlaceEntity(entityObj);
        }
    }

    //--------------------------------------------------------------
    // Place dynamiquement une seule entité (joueur ou monstre)
    [Server]
    public void PlaceEntity(GameObject entityObj)
    {
        if (entityObj == null) return;

        if (!entityObj.TryGetComponent(out Entity_StatistiqueCombat stats))
        {
            Debug.LogWarning("[Phase_Preparation] Impossible de placer l’entité (pas de Entity_StatistiqueCombat)");
            return;
        }

        int team = stats.team;
        GameObject tile = GetFreeTileForTeam(team);

        if (tile == null)
        {
            Debug.LogError($"[Phase_Preparation] Aucune case libre pour {entityObj.name} (équipe {team})");
            return;
        }

        Vector3 newPosition = tile.transform.position + Vector3.up * 0.5f;
        Quaternion newRotation = Quaternion.identity;

        if (team == 0)
            newRotation = Quaternion.Euler(rotationGreenTeamEuler);
        else if (team == 1)
            newRotation = Quaternion.Euler(rotationRedTeamEuler);

        entityObj.transform.position = newPosition;
        entityObj.transform.rotation = newRotation;

        if (entityObj.TryGetComponent(out Player_SetupNetworkCombat setup))
        {
            setup.SetInitialPosition(newPosition);
        }

        entityToTile[entityObj] = tile;
        tileToEntity[tile] = entityObj;

        Debug.Log($"[Phase_Preparation] Nouvelle entité placée : {entityObj.name} sur {tile.name}");
    }

    //--------------------------------------------------------------
    [Server]
    private GameObject GetFreeTileForTeam(int team)
    {
        foreach (GameObject tile in allTiles)
        {
            if (!tileToEntity.ContainsKey(tile) || tileToEntity[tile] != null)
                continue;

            if (tile.TryGetComponent(out Setup_NetworkTile setup))
            {
                if (team == 0 && setup.currentState == TileState.TeamGreen)
                    return tile;
                if (team == 1 && setup.currentState == TileState.TeamRed)
                    return tile;
            }
        }

        return null;
    }

    //--------------------------------------------------------------
    [Server]
    public void Debug_ShowEntityTileLinks()
    {
        Debug.Log($"-------------------------------------------------------------------");
        Debug.Log($"--- [DEBUG] Entités dans le dictionnaire ({entityToTile.Count}) ---");

        foreach (var pair in entityToTile)
        {
            string entityName = pair.Key != null ? pair.Key.name : "NULL";

            string tileName = pair.Value != null
                ? $"{pair.Value.name} ({pair.Value.GetComponent<Setup_NetworkTile>()?.tileX}, {pair.Value.GetComponent<Setup_NetworkTile>()?.tileY})"
                : "NULL";

            Debug.Log($" > {entityName} est sur {tileName}");
        }

        Debug.Log($"--- [DEBUG] Cases occupées ({tileToEntity.Count}) ---");

        foreach (var pair in tileToEntity)
        {
            string tileName = pair.Key != null
                ? $"{pair.Key.name} ({pair.Key.GetComponent<Setup_NetworkTile>()?.tileX}, {pair.Key.GetComponent<Setup_NetworkTile>()?.tileY})"
                : "NULL";

            string entityName = pair.Value != null ? pair.Value.name : "VIDE";
            Debug.Log($" > {tileName} contient : {entityName}");
        }

        Debug.Log($"-------------------------------------------------------------------");
    }

    [Server]
    public GameObject GetTileAtCoordinates(int x, int y)
    {
        foreach (GameObject tile in allTiles)
        {
            if (tile.TryGetComponent(out Setup_NetworkTile setup))
            {
                if (setup.tileX == x && setup.tileY == y)
                    return tile;
            }
        }
        return null;
    }

    [Server]
    public bool IsTileFree(GameObject tile)
    {
        return tileToEntity.ContainsKey(tile) && tileToEntity[tile] == null;
    }

    

}
