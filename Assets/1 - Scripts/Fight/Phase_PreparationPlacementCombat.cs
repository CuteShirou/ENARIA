using System.Collections.Generic;
using UnityEngine;
using Mirror;

//--------------------------------------------------------------
public class Phase_PreparationPlacementCombat : NetworkBehaviour
{
    [Header("Paramètres de génération de la grille")]
    public GameObject tilePrefab; // Prefab de la tuile
    public Transform gridCenter; // Point central de la grille (TileGridRoot)
    public Vector2 tileSpacing = new Vector2(1, 1); // Espacement horizontal / vertical
    public Vector3 gridRotation; // Rotation globale appliquée au centre

    private Combat_PhaseManager manager;
    private Data_FightMap mapData;

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
        }
    }

    //--------------------------------------------------------------
    [Server]
    private void GenerateGrid()
    {
        // Applique la rotation globale au parent
        gridCenter.rotation = Quaternion.Euler(gridRotation);

        // Calcul du décalage pour centrer la grille
        Vector3 offset = new Vector3(
            (mapData.width - 1) * tileSpacing.x / 2f,
            0,
            (mapData.height - 1) * tileSpacing.y / 2f
        );

        // On vérifie que le point d'encrage (TileGridRoot) a bien un NetworkIdentity
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
                tile.transform.SetParent(gridCenter);

                // Position locale centrée
                Vector3 localPos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y) - offset;
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity;

                // Configuration de la tuile
                NetworkTile tileNet = tile.GetComponent<NetworkTile>();

                if (tileNet != null)
                {
                    // Définir le type de case selon la map
                    if (mapData.greenTeamPositions.Contains(pos))
                        tileNet.currentState = TileState.TeamGreen;
                    else if (mapData.redTeamPositions.Contains(pos))
                        tileNet.currentState = TileState.TeamRed;
                    else if (mapData.interactiveObjectPositions.Contains(pos))
                        tileNet.currentState = TileState.Obstacle;
                    else
                        tileNet.currentState = TileState.None;

                    // Transmet au client l'identifiant réseau du parent (gridCenter)
                    tileNet.parentNetId = parentNetId.netId;
                }
                else
                {
                    Debug.LogWarning("[Phase_Preparation] Tuile sans script NetworkTile.");
                }

                // Spawn réseau de la tuile
                NetworkServer.Spawn(tile);
                Debug.Log($"[Grid] Tuile ({x},{y}) générée.");
            }
        }

        Debug.Log("[Phase_Preparation] Grille complète générée et centrée.");
    }
}
