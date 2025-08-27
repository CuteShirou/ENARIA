using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Monster_CombatController
/// [FR] IA minimale : au début de son tour -> attend 1s -> se déplace (utilise ses PM) -> 
///      quand il est sur la nouvelle case -> attend 1s -> termine son tour.
/// </summary>
[AddComponentMenu("Combat/Monster Combat Controller")]
public class Monster_CombatController : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   // [FR] Référence explicite (pas d'auto-find)
    public TileGrid_Manager tileGrid;          // [FR] Grille de combat (coordonnées & occupation)

    [Header("Movement")]
    public float moveSpeed = 5f;               // [FR] Vitesse visuelle du déplacement
    public bool isMoving = false;              // [FR] Lu par la phase pour interdire EndTurn pendant le mouvement

    // [FR] Etat interne (simple machine à états temps réel)
    private bool prevWasMyTurn = false;        // [FR] Détection front début/fin de tour
    private bool movedThisTurn = false;        // [FR] A-t-il déjà fait sa séquence de déplacement ?
    private bool waitingAfterMove = false;     // [FR] Attend-il la seconde d’après-mouvement ?
    private float waitTimer = 0f;              // [FR] Compteur d’attente (1s)

    private readonly Queue<Vector3> movementQueue = new(); // [FR] File des positions monde à suivre
    private Entity_StatistiqueCombat stats;    // [FR] Accès aux PM
    private GameObject plannedFinalTile;       // [FR] Tuile visée à la fin du déplacement (pour vérifier l'occupation)

    // =========================
    // Constructor / Destructor
    // =========================
    public Monster_CombatController() { /* Constructeur */ }
    ~Monster_CombatController() { /* Déconstructeur - non utilisé */ }

    private void Start()
    {
        // [FR] Références locales
        stats = GetComponent<Entity_StatistiqueCombat>();
        // [FR] Références externes fournies via Inspector (pas d'auto-find)
    }

    private void Update()
    {
        // [FR] Réfs indispensables
        if (phaseManager == null || phaseManager.phaseTurn == null || tileGrid == null || stats == null) return;

        bool myTurn = phaseManager.phaseTurn.IsMyTurn(gameObject);

        // [FR] Front de début de tour → reset de la mini IA + 1s d'attente
        if (myTurn && !prevWasMyTurn)
        {
            movementQueue.Clear();
            isMoving = false;
            movedThisTurn = false;
            waitingAfterMove = false;
            plannedFinalTile = null;

            waitTimer = 1f; // [FR] Attente initiale
        }

        // [FR] Front de fin de tour → nettoyage
        if (!myTurn && prevWasMyTurn)
        {
            movementQueue.Clear();
            isMoving = false;
            movedThisTurn = false;
            waitingAfterMove = false;
            plannedFinalTile = null;
            waitTimer = 0f;
        }

        prevWasMyTurn = myTurn;

        if (!myTurn) return; // [FR] Ne fait rien hors de son tour

        // [FR] Avance du déplacement visuel si nécessaire
        HandleMovement();

        // [FR] IA minimale
        RunTurnLogic();
    }

    // ---------------------------------------------------------------------
    // [FR] Déplacement visuel frame par frame (comme Player_Controller)
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

    // ---------------------------------------------------------------------
    // [FR] Séquence d'un tour : attente 1s -> déplacement -> attente 1s -> EndTurn
    private void RunTurnLogic()
    {
        // [FR] 1) Attente initiale (si demandée)
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        // [FR] 2) Si pas encore déplacé, on planifie et on lance le déplacement
        if (!movedThisTurn && !isMoving)
        {
            List<Vector2Int> path = PlanRandomWalk(stats.currentPM); // [FR] Essaie d'utiliser un max de PM
            if (path != null && path.Count > 0)
            {
                // [FR] Consomme les PM
                stats.SetPM(stats.currentPM - path.Count);

                // [FR] Convertit en positions monde
                movementQueue.Clear();
                for (int i = 0; i < path.Count; i++)
                {
                    var stepTile = tileGrid.GetTileAtCoordinates(path[i].x, path[i].y);
                    if (!stepTile) continue;
                    Vector3 wp = stepTile.transform.position; wp.y += 0.1f;
                    movementQueue.Enqueue(wp);

                    if (i == path.Count - 1)
                        plannedFinalTile = stepTile; // [FR] On mémorise la tuile cible finale
                }

                isMoving = movementQueue.Count > 0;
            }

            // [FR] Qu'il ait bougé ou non, on marque "déplacement traité"
            movedThisTurn = true;

            // [FR] Si aucun déplacement n'a été possible → on passera directement à l'attente finale
            if (!isMoving) waitingAfterMove = false; // sera réglé plus bas
            return;
        }

        // [FR] 3) Si on a fini de bouger, on vérifie que la tuile finale est bien occupée par le monstre
        if (movedThisTurn && !isMoving && !waitingAfterMove)
        {
            if (plannedFinalTile == null)
            {
                // [FR] Pas de mouvement ce tour → on peut enchaîner l'attente finale.
                waitTimer = 1f;
                waitingAfterMove = true;
                return;
            }

            // [FR] Vérifie l'occupation via la grille (Phase met à jour les dicos en temps réel)
            var occ = tileGrid.GetEntityOnTile(plannedFinalTile);
            if (occ == gameObject)
            {
                waitTimer = 1f;          // [FR] Attente d'après-mouvement
                waitingAfterMove = true;
            }
            // [FR] Sinon: on attend le prochain frame (la Phase va enregistrer la nouvelle tuile)
            return;
        }

        // [FR] 4) Après l'attente d'après-mouvement → Fin de tour
        if (waitingAfterMove && waitTimer <= 0f)
        {
            phaseManager.phaseTurn.EndTurn();   // [FR] Termine le tour proprement
            waitingAfterMove = false;
        }
    }

    // ---------------------------------------------------------------------
    // [FR] Planifie un "random walk" jusqu'à PM cases (quand possible), en évitant les cases occupées.
    private List<Vector2Int> PlanRandomWalk(int maxSteps)
    {
        var path = new List<Vector2Int>();
        if (maxSteps <= 0) return path;

        Vector2Int cursor = GetCurrentCoord();

        // [FR] Pour chaque PM, on essaie un pas vers un voisin libre (ordre aléatoire)
        for (int step = 0; step < maxSteps; step++)
        {
            // 4 directions cardinales mélangées
            var dirs = ShuffleDirections(new[]
            {
                new Vector2Int( 1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 0,-1),
            });

            bool moved = false;
            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int next = cursor + dirs[i];

                var tileObj = tileGrid.GetTileAtCoordinates(next.x, next.y);
                if (!tileObj) continue; // hors grille

                var occ = tileGrid.GetEntityOnTile(tileObj);
                if (occ && occ != gameObject) continue; // occupée par autre chose

                // [FR] Pas valide → on l'ajoute au chemin et on avance le curseur
                path.Add(next);
                cursor = next;
                moved = true;
                break;
            }

            if (!moved) break; // [FR] Encerclé / pas de case libre autour → on s'arrête
        }

        return path;
    }

    // [FR] Mélange simple d'un tableau de directions (Fisher–Yates light)
    private Vector2Int[] ShuffleDirections(Vector2Int[] dirs)
    {
        for (int i = 0; i < dirs.Length; i++)
        {
            int j = Random.Range(i, dirs.Length);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
        }
        return dirs;
    }

    // [FR] Récupère la coordonnée (x,y) de la tuile actuelle du monstre
    private Vector2Int GetCurrentCoord()
    {
        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile st))
            return new Vector2Int(st.tileX, st.tileY);

        // [FR] Fallback (devrait peu arriver) : plus proche tuile
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
}
