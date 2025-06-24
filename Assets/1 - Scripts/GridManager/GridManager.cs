using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] GameObject BasicTile;
    [SerializeField] int GridHeight = 20;
    [SerializeField] int GridWidth = 20;
    [SerializeField] float TileSize = 1f;
    [SerializeField] float TileSpacing = 0.1f;

    public Dictionary<Vector2Int, GameObject> TileMap = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        float spacing = TileSize + TileSpacing;

        // Calcule la taille totale de la grille
        float totalWidth = GridWidth * spacing;
        float totalHeight = GridHeight * spacing;

        // Calcule le point de départ pour centrer
        Vector3 gridOrigin = transform.position - new Vector3(totalWidth / 2f, 0, totalHeight / 2f);

        for (int x = 0; x < GridWidth; x++)
        {
            for (int z = 0; z < GridHeight; z++)
            {
                Vector3 position = gridOrigin + new Vector3(x * spacing, 0.1f, z * spacing);
                GameObject newTile = Instantiate(BasicTile, position, Quaternion.identity, transform);

                Vector2Int newTilePos = new Vector2Int(x, z);
                TileMap[newTilePos] = newTile;

                TileCoord tileCoord = newTile.GetComponent<TileCoord>();
                if (tileCoord != null)
                {
                    tileCoord.SetCoord(x, z);
                    newTile.name = $"Tile ({x}, {z})";
                }
            }
        }
    }

}
