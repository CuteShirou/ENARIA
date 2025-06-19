using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode
{
    public Vector2Int Coord;
    public PathfindingNode Parent;
    public int G; // Distance depuis le d�part
    public int H; // Heuristique (distance jusqu�� la cible)
    public int F => G + H;

    public PathfindingNode(Vector2Int coord, PathfindingNode parent, int g, int h)
    {
        Coord = coord;
        Parent = parent;
        G = g;
        H = h;
    }
}

