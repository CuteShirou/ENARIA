using System.Collections.Generic;
using UnityEngine;

public class Player_CombatController : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   //   pour tester IsMyTurn()
    public TileGrid_Manager tileGrid;          //   ta grille de combat

    [Header("Movement")]
    public float moveSpeed = 6f;               //   vitesse visuelle
    public bool isMoving = false;              //   flag public lu par la phase (anti EndTurn)

    private readonly Queue<Vector3> movementQueue = new();
    private Entity_StatistiqueCombat stats;

    private void Awake()
    {
        if (!phaseManager) phaseManager = FindAnyObjectByType<Combat_PhaseManager>();
        if (!tileGrid && phaseManager) tileGrid = phaseManager.tileGrid;
    }

    private void Start()
    {
        stats = GetComponent<Entity_StatistiqueCombat>();
    }

    private void Update()
    {
        //   Ignore hors-tour
        if (phaseManager == null || phaseManager.phaseTurn == null) return;
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        HandleMovement();
        HandleClick();
    }

    // ---------------------------------------------------------------------
    private void HandleMovement()
    {
        if (!isMoving || movementQueue.Count == 0) return;

        Vector3 target = movementQueue.Peek();
        float step = moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) <= 0.01f)
        {
            movementQueue.Dequeue();
            if (movementQueue.Count == 0) isMoving = false;
        }
    }

    private void HandleClick()
    {
        if (isMoving) return;
        if (stats == null || stats.currentPM <= 0) return;
        if (!Camera.main || tileGrid == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
            if (!hit.collider.TryGetComponent(out SetupTile targetTile)) return;

            Vector2Int from = GetCurrentCoord();
            Vector2Int to = new Vector2Int(targetTile.tileX, targetTile.tileY);

            if (from == to)
            {
                Debug.Log("Déjà sur cette case.");
                return;
            }

            var destObj = tileGrid.GetTileAtCoordinates(to.x, to.y);
            if (!destObj)
            {
                Debug.Log("Case invalide.");
                return;
            }
            var occ = tileGrid.GetEntityOnTile(destObj);
            if (occ && occ != gameObject)
            {
                Debug.Log("Case occupée.");
                return;
            }

            //   Utilise ton pathfinder si dispo (remplace la ligne ci-dessous)
            List<Vector2Int> path = TryFindPath(from, to, stats.currentPM);

            if (path == null || path.Count == 0)
            {
                Debug.Log("Aucun chemin possible ou PM insuffisants.");
                return;
            }

            //   Consomme les PM selon longueur du chemin
            stats.SetPM(stats.currentPM - path.Count);

            //   File des positions monde
            movementQueue.Clear();
            for (int i = 0; i < path.Count; i++)
            {
                var stepTile = tileGrid.GetTileAtCoordinates(path[i].x, path[i].y);
                if (!stepTile) continue;
                Vector3 wp = stepTile.transform.position; wp.y += 0.1f;
                movementQueue.Enqueue(wp);
            }

            isMoving = movementQueue.Count > 0;
        }
    }

    // ---------------------------------------------------------------------
    private List<Vector2Int> TryFindPath(Vector2Int from, Vector2Int to, int maxSteps)
    {
        //   Exemple si tu as un utilitaire : return TileGrid_Pathfinder.FindPath(tileGrid, from, to, maxSteps);

        //   Fallback simple Manhattan
        var result = new List<Vector2Int>();
        Vector2Int cursor = from;

        while (cursor != to && result.Count < maxSteps)
        {
            Vector2Int next = cursor;

            if (to.x > cursor.x) next = new Vector2Int(cursor.x + 1, cursor.y);
            else if (to.x < cursor.x) next = new Vector2Int(cursor.x - 1, cursor.y);
            else if (to.y > cursor.y) next = new Vector2Int(cursor.x, cursor.y + 1);
            else if (to.y < cursor.y) next = new Vector2Int(cursor.x, cursor.y - 1);

            var tileObj = tileGrid.GetTileAtCoordinates(next.x, next.y);
            if (!tileObj) break;

            var occ = tileGrid.GetEntityOnTile(tileObj);
            if (occ && occ != gameObject) break;

            result.Add(next);
            cursor = next;
        }
        return result;
    }

    private Vector2Int GetCurrentCoord()
    {
        if (tileGrid == null) return Vector2Int.zero;

        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile st))
            return new Vector2Int(st.tileX, st.tileY);

        // fallback : plus proche
        Vector2Int best = Vector2Int.zero;
        float min = float.MaxValue;
        var tiles = tileGrid.GetAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            if (!t || !t.TryGetComponent(out SetupTile s)) continue;
            float d = Vector3.Distance(transform.position, t.transform.position);
            if (d < min) { min = d; best = new Vector2Int(s.tileX, s.tileY); }
        }
        return best;
    }

    //   À lier au bouton "Passe Tour" (OnClick)
    public void OnClickPassTurn()
    {
        //   Sécurité : on ne fait rien si la phase n'est pas prête
        if (phaseManager == null || phaseManager.phaseTurn == null) return;

        //   On ne peut passer le tour que si c'est bien mon tour
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        //   On évite de couper un déplacement en cours
        if (isMoving) return;

        //   Nettoie toute file de déplacement résiduelle (visuel propre)
        movementQueue.Clear();
        isMoving = false;

        //   Demande à la phase de passer au prochain combattant.
        //   Adapte le nom exact de la méthode selon ton Phase_TurnByTurn :
        phaseManager.phaseTurn.EndTurn();
        // phaseManager.phaseTurn.EndTurnForCurrent();         // <- si ta méthode s'appelle comme ceci
        // phaseManager.phaseTurn.RequestEndTurn(gameObject);  // <- si elle attend l'actor en paramètre
    }

}
