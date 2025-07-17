using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Mirror;

public class CombatController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public Grid gridManager;
    private Camera mainCamera;

    private Queue<Vector3> movementQueue = new Queue<Vector3>();
    private Vector3 currentTarget;

    public bool isMoving = false;
    private CombatStats stats;
    private TileCoord currentTile;

    private void Start()
    {
        mainCamera = Camera.main;
        stats = GetComponent<CombatStats>();

        if (stats == null)
            Debug.LogError($"{name} n'a pas de CombatStats attaché !");

        currentTile = FindClosestTile();
        if (currentTile != null)
            currentTile.SetOccupant(gameObject);
    }

    private void Update()
    {
        if (!isServer)
            return;

        HandleMovement();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

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

                CmdRequestMove(targetTile.Coord);
            }
        }
    }

    [Command]
    private void CmdRequestMove(Vector2Int targetCoord)
    {
        if (isMoving || stats == null || stats.currentPM <= 0)
            return;

        if (!gridManager.TileMap.ContainsKey(targetCoord))
            return;

        TileCoord destinationTile = gridManager.TileMap[targetCoord].GetComponent<TileCoord>();
        if (destinationTile == null || destinationTile.IsOccupied)
            return;

        List<Vector2Int> path = AStarPathfinder.FindPath(GetCurrentCoord(), targetCoord, gridManager, stats.currentPM);
        if (path == null)
            return;

        if (currentTile != null)
            currentTile.ClearOccupant();

        currentTile = destinationTile;
        destinationTile.SetOccupant(gameObject);

        stats.currentPM -= path.Count;

        movementQueue.Clear();
        foreach (Vector2Int step in path)
        {
            Vector3 worldPos = gridManager.TileMap[step].transform.position + new Vector3(0, 0.1f, 0);
            movementQueue.Enqueue(worldPos);
        }

        isMoving = true;
    }

    public void AssignToTile(TileCoord newTile)
    {
        if (currentTile != null)
            currentTile.ClearOccupant();

        currentTile = newTile;
        currentTile.SetOccupant(gameObject);
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

    public Vector2Int GetCurrentCoord()
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

    public TileCoord GetCurrentTile()
    {
        return currentTile;
    }

    [Command]
    public void CmdRequestTileChange(NetworkIdentity tileNetId)
    {
        if (!isServer) return;
        if (isMoving) return;
        if (stats == null) return;
        if (stats.currentPM <= 0) return;

        TileCoord newTile = tileNetId.GetComponent<TileCoord>();
        if (newTile == null) return;
        if (newTile == currentTile) return;
        if (!FindAnyObjectByType<CombatPreparationManager>().mapData.greenTeamPositions.Contains(newTile.Coord))
            return;

        CombatPreparationManager prep = FindAnyObjectByType<CombatPreparationManager>();
        if (prep != null)
        {
            prep.TryMovePlayerTo(newTile);
        }
    }

}
