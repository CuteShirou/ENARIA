using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoord : MonoBehaviour
{
    [SerializeField] Material highlight;
    [SerializeField] Material normal;
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int Coord;
    public GameObject occupant;
    public bool IsOccupied => occupant != null;
    public void SetCoord(int x, int y)
    {
        X = x;
        Y = y;
        Coord = new Vector2Int(X, Y);
    }
    private void OnMouseEnter()
    {
        gameObject.GetComponent<Renderer>().sharedMaterial = highlight;
    }
    private void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().sharedMaterial = normal;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked at coordinates: ({X}, {Y}) and cartesians: ({gameObject.GetComponent<Transform>().position.x} ; {gameObject.GetComponent<Transform>().position.z})");
    }
    public void SetOccupant(GameObject entity)
    {
        occupant = entity;
    }

    public void ClearOccupant()
    {
        occupant = null;
    }
}
