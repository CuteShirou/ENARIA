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
    private CombatStats stats;
    private TileCoord currentTile;

    void Start()
    {
        mainCamera = Camera.main;
        stats = GetComponent<CombatStats>();

        if (stats == null)
            Debug.LogError($"{name} n'a pas de CombatStats attaché !");

        currentTile = FindClosestTile();
        if (currentTile != null)
            currentTile.SetOccupant(gameObject);
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
                TileCoord targetTile = hit.collider.GetComponent<TileCoord>();
                if (targetTile == null) return;

                Vector2Int fromCoord = GetCurrentCoord();
                Vector2Int toCoord = targetTile.Coord;

                List<Vector2Int> path = AStarPathfinder.FindPath(fromCoord, toCoord, gridManager, stats.currentPM);
                TileCoord destinationTile = gridManager.TileMap[toCoord].GetComponent<TileCoord>();
                if (destinationTile != null && destinationTile.occupant == gameObject)
                {
                    Debug.Log("Tu es déjà sur cette case.");
                    return;
                }
                else if (destinationTile != null && destinationTile.IsOccupied)
                {
                    Debug.Log("La case est déjà occupée !");
                    return;
                }
                else if (path == null)
                {
                    Debug.Log("Aucun chemin trouvé ou trop loin !");
                    return;
                }
                

                // Libère la case actuelle
                if (currentTile != null)
                    currentTile.ClearOccupant();

                currentTile = gridManager.TileMap[toCoord].GetComponent<TileCoord>();
                currentTile.SetOccupant(gameObject);

                stats.currentPM -= path.Count;

                movementQueue.Clear();
                foreach (Vector2Int step in path)
                {
                    Vector3 worldPos = gridManager.TileMap[step].transform.position + new Vector3(0, 0.5f, 0);
                    movementQueue.Enqueue(worldPos);
                }

                isMoving = true;
            }
        }
    }


    private List<TileCoord> GetValidLPath(Vector2Int from, Vector2Int to)
    {
        List<TileCoord> path = new List<TileCoord>();

        // Cas 1 : Horizontal puis Vertical
        Vector2Int inter1 = new Vector2Int(to.x, from.y);
        if (IsTileFree(inter1) && IsTileFree(to))
        {
            path.Add(gridManager.TileMap[inter1].GetComponent<TileCoord>());
            path.Add(gridManager.TileMap[to].GetComponent<TileCoord>());
            return path;
        }

        // Cas 2 : Vertical puis Horizontal
        Vector2Int inter2 = new Vector2Int(from.x, to.y);
        if (IsTileFree(inter2) && IsTileFree(to))
        {
            path.Add(gridManager.TileMap[inter2].GetComponent<TileCoord>());
            path.Add(gridManager.TileMap[to].GetComponent<TileCoord>());
            return path;
        }

        return null;
    }

    private bool IsTileFree(Vector2Int coord)
    {
        if (!gridManager.TileMap.ContainsKey(coord)) return false;
        TileCoord tile = gridManager.TileMap[coord].GetComponent<TileCoord>();
        return tile != null && !tile.IsOccupied;
    }

    private TileCoord FindClosestTile()
    {
        float minDist = float.MaxValue;
        TileCoord closest = null;

        foreach (var tileGO in gridManager.TileMap.Values)
        {
            float d = Vector3.Distance(transform.position, tileGO.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = tileGO.GetComponent<TileCoord>();
            }
        }

        return closest;
    }

    private Vector2Int GetCurrentCoord()
    {
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
