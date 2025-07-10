using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject BasicTile;
    [SerializeField] private int GridHeight = 10;
    [SerializeField] private int GridWidth = 10;
    [SerializeField] private float TileSize = 1f;
    [SerializeField] private float TileSpacing = 0.05f;

    // Accesseurs publics en lecture seule
    public float TileSizeValue => TileSize;
    public float TileSpacingValue => TileSpacing;
    public int GridWidthValue => GridWidth;
    public int GridHeightValue => GridHeight;


    public Dictionary<Vector2Int, GameObject> TileMap = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        //GenerateGrid();
        this.transform.Rotate(0, 45, 0);
    }

    public void ClearGrid()
    { 
        foreach (var tile in TileMap)
        {
            tile.Value.gameObject.GetComponent<Renderer>().sharedMaterial = BasicTile.GetComponent<TileCoord>().normal;
        }
    }

    public void ClearOccupant()
    {
        foreach (var tile in TileMap)
        {
            tile.Value.GetComponent<TileCoord>().ClearOccupant();
        }
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
