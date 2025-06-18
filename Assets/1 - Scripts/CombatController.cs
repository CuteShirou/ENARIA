using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Grid gridManager;
    private Camera mainCamera;

    private Queue<Vector3> movementQueue = new Queue<Vector3>();
    private Vector3 currentTarget;

    private bool isMoving = false;
    public ReachableTilesHighlighter highlighter;
    private CombatStats stats;

    void Start()
    {
        mainCamera = Camera.main;
        stats = GetComponent<CombatStats>();

        if (stats == null)
            Debug.LogError($"{name} n'a pas de CombatStats attaché !");
    }

    void Update()
    {
        HandleMovement();
        HandleClick();
    }

    private void HandleMovement()
    {
        if (isMoving && movementQueue.Count > 0)
        {
            currentTarget = movementQueue.Peek();
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, step);

            if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
            {
                movementQueue.Dequeue();
                if (movementQueue.Count == 0)
                    isMoving = false;
            }
        }
    }

    private void HandleClick()
    {
        if (isMoving || stats == null || stats.currentPM <= 0)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                TileCoord tile = hit.collider.GetComponent<TileCoord>();
                if (tile != null)
                {
                    if (!gridManager.TileMap.ContainsKey(tile.Coord))
                    {
                        Debug.LogWarning($"Coordonnée {tile.Coord} introuvable dans la TileMap !");
                        return;
                    }

                    Vector3 destination = gridManager.TileMap[tile.Coord].transform.position + new Vector3(0, 0.5f, 0);

                    // Calcule de la distance en cases
                    Vector2Int fromCoord = GetCurrentCoord();
                    Vector2Int toCoord = tile.Coord;
                    int distance = Mathf.Abs(fromCoord.x - toCoord.x) + Mathf.Abs(fromCoord.y - toCoord.y);

                    if (distance <= stats.currentPM)
                    {
                        stats.currentPM -= distance;

                        // Créer le chemin en L
                        Vector3 intermediate1 = new Vector3(destination.x, transform.position.y, transform.position.z);
                        Vector3 intermediate2 = new Vector3(destination.x, transform.position.y, destination.z);

                        movementQueue.Clear();
                        movementQueue.Enqueue(intermediate1);
                        movementQueue.Enqueue(intermediate2);
                        isMoving = true;
                    }
                    else
                    {
                        Debug.Log("Pas assez de PM !");
                    }
                }
            }
        }
    }

    private Vector2Int GetCurrentCoord()
    {
        // Recherche la case la plus proche
        Vector3 pos = transform.position;
        float minDist = float.MaxValue;
        Vector2Int closest = Vector2Int.zero;

        foreach (var kvp in gridManager.TileMap)
        {
            float d = Vector3.Distance(pos, kvp.Value.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = kvp.Key;
            }
        }
        return closest;
    }
}
