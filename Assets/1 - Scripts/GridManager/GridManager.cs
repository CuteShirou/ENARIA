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

    public Dictionary<Vector2Int, GameObject> TileMap = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {

        float GOX = gameObject.GetComponent<Transform>().position.x;
        float GOY = gameObject.GetComponent<Transform>().position.y;
        float GOZ = gameObject.GetComponent<Transform>().position.z;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int z = 0; z < GridHeight; z++)
            {
                float spacing = TileSize + TileSpacing;
                Vector3 position = new Vector3(GOX + x * spacing, GOY + 0.1f , GOZ + z * spacing);
                GameObject newTile = Instantiate(BasicTile, position, Quaternion.identity, transform);
                Vector2Int newTilePos = new Vector2Int(x,z);
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
