using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class CombatMapLoader : NetworkBehaviour
{
    [Header("Carte à utiliser")]
    public CombatMapData mapData;

    [Header("Références")]
    public Grid grid;
    public GameObject tilePrefab;
    public Material matNormal;
    public Material matGreen;
    public Material matRed;
    public Material matBlue;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (mapData != null && grid != null)
        {
            GenerateGridFromMap();
        }
        else
        {
            Debug.LogError("CombatMapLoader : données ou grille manquantes !");
        }
    }

    [Server]
    public void GenerateGridFromMap()
    {
        float spacing = grid.TileSizeValue + grid.TileSpacingValue;
        float totalWidth = mapData.width * spacing;
        float totalHeight = mapData.height * spacing;

        Vector3 gridOrigin = grid.transform.position - new Vector3(totalWidth / 2f, 0, totalHeight / 2f);

        grid.TileMap.Clear();

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                Vector3 pos = gridOrigin + new Vector3(x * spacing, 0.1f, y * spacing);

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);
                tile.name = $"Tile ({x},{y})";

                TileCoord tc = tile.GetComponent<TileCoord>();
                if (tc != null)
                {
                    tc.SetCoord(x, y);

                    tc.normal = matNormal;
                    tc.green = matGreen;
                    tc.red = matRed;
                    tc.blue = matBlue;

                    if (mapData.greenTeamPositions.Contains(coord))
                        tc.SetTeamColor("green");
                    else if (mapData.redTeamPositions.Contains(coord))
                        tc.SetTeamColor("red");
                    else if (mapData.interactiveObjectPositions.Contains(coord))
                        tc.SetTeamColor("blue");
                    else
                        tc.SetTeamColor("normal");
                }

                NetworkServer.Spawn(tile);
                tile.transform.SetParent(grid.transform);

                grid.TileMap[coord] = tile;
            }
        }
    }
}
