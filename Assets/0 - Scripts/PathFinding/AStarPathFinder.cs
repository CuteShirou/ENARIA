using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, Grid grid, int maxPM)
    {
        Dictionary<Vector2Int, GameObject> map = grid.TileMap;
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        List<PathfindingNode> openList = new List<PathfindingNode>();

        PathfindingNode startNode = new PathfindingNode(start, null, 0, Heuristic(start, goal));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Prend le noeud avec le plus petit F
            openList.Sort((a, b) => a.F.CompareTo(b.F));
            PathfindingNode current = openList[0];
            openList.RemoveAt(0);

            if (current.Coord == goal)
                return ReconstructPath(current, maxPM);

            closedSet.Add(current.Coord);

            foreach (Vector2Int neighbor in GetNeighbors(current.Coord, map))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                // Ignore les cases occupées sauf si c’est la destination
                TileCoord tile = map[neighbor].GetComponent<TileCoord>();
                if (tile == null || (tile.IsOccupied && neighbor != goal))
                    continue;

                int tentativeG = current.G + 1;

                PathfindingNode existing = openList.Find(n => n.Coord == neighbor);
                if (existing == null)
                {
                    PathfindingNode newNode = new PathfindingNode(neighbor, current, tentativeG, Heuristic(neighbor, goal));
                    openList.Add(newNode);
                }
                else if (tentativeG < existing.G)
                {
                    existing.Parent = current;
                    existing.G = tentativeG;
                }
            }
        }

        return null; // Aucun chemin trouvé
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan
    }

    private static List<Vector2Int> ReconstructPath(PathfindingNode node, int maxPM)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (node != null)
        {
            path.Insert(0, node.Coord);
            node = node.Parent;
        }

        // Supprimer le point de départ
        if (path.Count > 0)
            path.RemoveAt(0);

        // Limiter au nombre de PM
        if (path.Count > maxPM)
            return null;

        return path;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int coord, Dictionary<Vector2Int, GameObject> map)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = coord + dir;
            if (map.ContainsKey(neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }
}
