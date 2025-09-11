using System.Collections.Generic;
using UnityEngine;

public class Player_CombatController : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   // pour tester IsMyTurn()
    public TileGrid_Manager tileGrid;          // grille de combat

    [Header("Movement")]
    public float moveSpeed = 6f;               // vitesse visuelle
    public bool isMoving = false;              // flag public lu par la phase (anti EndTurn)

    private readonly Queue<Vector3> movementQueue = new();
    private Entity_StatistiqueCombat stats;

    [Header("Animation")]
    public Entity_Animations entityAnim;       // contrôleur d'animations 3D (Player/Monstre)

    [Header("Orientation")]
    public bool rotateTowardsTarget = true;        // Active/désactive la rotation vers la tuile de destination
    public bool instantTurnAtStepStart = true;     // Si true, pivot instantané au début de chaque étape
    public float rotateSpeedDeg = 540f;            // Vitesse de rotation si non instantané (degrés/seconde)
    public float rotationOffsetY = 0f;             // Offset Y pour corriger un prefab mal orienté

    void Awake()
    {
        // Récupère les refs si non assignées
        if (!phaseManager) phaseManager = FindAnyObjectByType<Combat_PhaseManager>();
        if (!tileGrid && phaseManager) tileGrid = phaseManager.tileGrid;
    }

    void Start()
    {
        // Récupère le composant de stats
        stats = GetComponent<Entity_StatistiqueCombat>();

        // Récupère l'anim s'il n'est pas déjà assigné dans l'Inspector
        if (entityAnim == null) TryGetComponent(out entityAnim);
    }

    void Update()
    {
        // Ne réagit que si c'est le tour de ce joueur
        if (phaseManager == null || phaseManager.phaseTurn == null) return;
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        HandleMovement(); // Gère la progression vers la prochaine cible
        HandleClick();    // Gère le clic pour empiler un nouveau chemin
    }

    // Avance vers la cible courante, sans jamais viser un Y différent du Y actuel
    void HandleMovement()
    {
        if (!isMoving || movementQueue.Count == 0) return;

        Vector3 target = movementQueue.Peek();

        // Sécurité : impose le Y courant du joueur à la cible, pour ne bouger qu'en X/Z
        target = new Vector3(target.x, transform.position.y, target.z);

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        // Tant qu'on est en déplacement, s'assurer que l'animation Walk reste active
        if (entityAnim != null && entityAnim.isActiveAndEnabled)
            entityAnim.SetWalk(true);

        // Pendant le mouvement, si on ne pivote pas instantanément, on oriente progressivement vers la cible
        if (rotateTowardsTarget && !instantTurnAtStepStart)
            RotateTowards(target, false);

        // Test d'arrivée en XZ uniquement (ignore toute variation de Y)
        Vector2 curXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 tgtXZ = new Vector2(target.x, target.z);
        if ((curXZ - tgtXZ).sqrMagnitude <= 0.0004f) // ~2cm^2
        {
            movementQueue.Dequeue();

            // Si une nouvelle étape existe, et qu'on souhaite pivoter instantanément au début d'étape, on l'applique ici
            if (movementQueue.Count > 0 && rotateTowardsTarget && instantTurnAtStepStart)
            {
                Vector3 next = movementQueue.Peek();
                next = new Vector3(next.x, transform.position.y, next.z);
                RotateTowards(next, true);
            }

            if (movementQueue.Count == 0)
            {
                isMoving = false;

                // Arrêt de l'animation Walk à l'arrivée
                if (entityAnim != null && entityAnim.isActiveAndEnabled)
                    entityAnim.StopWalk();
            }
        }
    }

    // Sur clic gauche, calcule un chemin puis empile des cibles monde "(tile.x, player.y, tile.z)"
    void HandleClick()
    {
        // Ignore pendant un mouvement ou si pas de PM
        if (isMoving) return;
        if (stats == null || stats.currentPM <= 0) return;
        if (!Camera.main || tileGrid == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Raycast souris vers la scène
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
            if (!hit.collider.TryGetComponent(out SetupTile targetTile)) return;

            // Coordonnées de départ/arrivée
            Vector2Int from = GetCurrentCoord();
            Vector2Int to = new Vector2Int(targetTile.tileX, targetTile.tileY);

            // Déjà sur place
            if (from == to)
            {
                Debug.Log("Déjà sur cette case.");
                return;
            }

            // Validité et occupation de la case cible
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

            // Calcule un chemin borné par le nombre de PM disponibles
            List<Vector2Int> path = TryFindPath(from, to, stats.currentPM);
            if (path == null || path.Count == 0)
            {
                Debug.Log("Aucun chemin possible ou PM insuffisants.");
                return;
            }

            // Consomme les PM selon la longueur du chemin
            stats.SetPM(stats.currentPM - path.Count);

            // Affiche la pop-up "- X PM" si présente
            var popup = GetComponent<Popup_DisplayNumber>();
            if (popup != null) popup.ShowPM(path.Count);

            // Prépare la file de cibles monde en conservant le Y courant
            movementQueue.Clear();
            float y = transform.position.y; // altitude actuelle à conserver pour tout le trajet
            for (int i = 0; i < path.Count; i++)
            {
                var stepTile = tileGrid.GetTileAtCoordinates(path[i].x, path[i].y);
                if (!stepTile) continue;

                Vector3 tilePos = stepTile.transform.position;
                Vector3 wp = new Vector3(tilePos.x, y, tilePos.z); // cible en XZ uniquement
                movementQueue.Enqueue(wp);
            }

            // Lance le mouvement si au moins une étape
            isMoving = movementQueue.Count > 0;

            // Démarre immédiatement l'animation Walk si un déplacement commence
            if (isMoving && entityAnim != null && entityAnim.isActiveAndEnabled)
                entityAnim.PlayWalk();

            // Oriente immédiatement vers la première cible si demandé
            if (isMoving && rotateTowardsTarget && instantTurnAtStepStart)
            {
                Vector3 first = movementQueue.Peek();
                first = new Vector3(first.x, transform.position.y, first.z);
                RotateTowards(first, true);
            }
        }
    }

    // Fallback Manhattan borné par maxSteps (remplace si tu as un pathfinder)
    List<Vector2Int> TryFindPath(Vector2Int from, Vector2Int to, int maxSteps)
    {
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

    // Détermine la coordonnée tuile actuelle du joueur (dico ou plus proche)
    Vector2Int GetCurrentCoord()
    {
        if (tileGrid == null) return Vector2Int.zero;

        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile st))
            return new Vector2Int(st.tileX, st.tileY);

        // Fallback : recherche de la tuile la plus proche
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

    // À lier au bouton "Passe Tour" (OnClick)
    public void OnClickPassTurn()
    {
        // Sécurité basique
        if (phaseManager == null || phaseManager.phaseTurn == null) return;
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;
        if (isMoving) return; // on évite de couper un mouvement en cours

        movementQueue.Clear();
        isMoving = false;

        // S'assure d'arrêter l'animation Walk si besoin
        if (entityAnim != null && entityAnim.isActiveAndEnabled)
            entityAnim.StopWalk();

        phaseManager.phaseTurn.EndTurn();
    }

    // Oriente le GameObject vers une position monde cible (Y ignoré), avec option instantanée
    // - Calcul du yaw sur le plan XZ
    // - Ajout d'un offset Y pour corriger l'orientation de base du prefab
    void RotateTowards(Vector3 worldTarget, bool instant)
    {
        if (!rotateTowardsTarget) return;

        Vector3 dir = worldTarget - transform.position;
        dir.y = 0f; // ignore la pente verticale

        if (dir.sqrMagnitude < 0.000001f) return; // évite les NaN

        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        yaw += rotationOffsetY;

        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);

        if (instant || rotateSpeedDeg <= 0f)
        {
            transform.rotation = targetRot; // pivot franc immédiat
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeedDeg * Time.deltaTime
            );
        }
    }
}
