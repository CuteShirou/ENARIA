using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Combat Map", menuName = "Game Creation Tool/Combat Map")]
public class CombatMapData : ScriptableObject
{
    public string mapName;
    public int width;
    public int height;

    public List<Vector2Int> greenTeamPositions = new List<Vector2Int>();
    public List<Vector2Int> redTeamPositions = new List<Vector2Int>();
    public List<Vector2Int> interactiveObjectPositions = new List<Vector2Int>();
}
