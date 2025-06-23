using UnityEngine;
using System.Collections.Generic;

public class ReachableTilesHighlighter : MonoBehaviour
{
    public Grid gridManager;
    public Material highlightMat;
    private Dictionary<GameObject, Material> originalMats = new Dictionary<GameObject, Material>();
    private List<GameObject> currentHighlighted = new List<GameObject>();

    public void HighlightReachableTiles(Vector2Int fromCoord, int maxRange)
    {
        ClearHighlight();

        foreach (var pair in gridManager.TileMap)
        {
            Vector2Int coord = pair.Key;
            GameObject tile = pair.Value;
            int distance = Mathf.Abs(coord.x - fromCoord.x) + Mathf.Abs(coord.y - fromCoord.y); // distance de Manhattan

            if (distance <= maxRange)
            {
                Renderer r = tile.GetComponent<Renderer>();
                if (r != null)
                {
                    originalMats[tile] = r.material;
                    r.material = highlightMat;
                    currentHighlighted.Add(tile);
                }
            }
        }
    }

    public void ClearHighlight()
    {
        foreach (var tile in currentHighlighted)
        {
            if (tile != null && originalMats.ContainsKey(tile))
            {
                tile.GetComponent<Renderer>().material = originalMats[tile];
            }
        }

        currentHighlighted.Clear();
        originalMats.Clear();
    }
}
