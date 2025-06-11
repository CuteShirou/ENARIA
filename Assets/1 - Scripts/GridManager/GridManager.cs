using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] GameObject BasicTile;
    [SerializeField] int GridHeight = 10;
    [SerializeField] int GridWidth = 10;
    [SerializeField] float TileSize = 1f;
    [SerializeField] float TileSpacing = 0.1f;


    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                float spacing = TileSize + TileSpacing;
                Vector3 position = new Vector3(x * spacing, 0.1f, y * spacing);
                GameObject newTile = Instantiate(BasicTile, position, Quaternion.identity, transform);

                TileCoord tileCoord = newTile.GetComponent<TileCoord>();
                if (tileCoord != null)
                {
                    tileCoord.SetCoord(x, y);
                    newTile.name = $"Tile ({x}, {y})";
                }
            }
        }
    }
}
