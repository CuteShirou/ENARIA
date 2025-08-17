using UnityEngine;

public class Player_ControllerPhasePreparation : MonoBehaviour
{
    [Header("Référence scène (Drag & Drop)")]
    [SerializeField] private Combat_PhaseManager combatManager;

    [Header("Fallback (si pas de Drag & Drop)")]
    [SerializeField] private bool autoFindManagerIfNull = true;

    [Header("Clicks globaux")]
    [SerializeField] private bool listenGlobalTileClicks = true;
    [SerializeField] private bool ignoreClicksWhenReady = true;

    private Entity_StatistiqueCombat stats;

    private void Awake()
    {
        TileClickInput.EnsureExists();

        if (combatManager == null && autoFindManagerIfNull)
        {
            combatManager = FindAnyObjectByType<Combat_PhaseManager>(FindObjectsInactive.Exclude);
            if (combatManager == null)
                Debug.LogWarning("[PREPA] Aucun Combat_PhaseManager trouvé automatiquement.");
        }

        TryCacheStats();
    }

    private void OnEnable()
    {
        if (listenGlobalTileClicks)
            TileClickInput.OnTileClicked += HandleTileClicked;
    }

    private void OnDisable()
    {
        if (listenGlobalTileClicks)
            TileClickInput.OnTileClicked -= HandleTileClicked;
    }

    private void TryCacheStats()
    {
        if (stats == null)
            TryGetComponent(out stats);
    }

    // Option: API directe si tu veux appeler depuis un autre script.
    public void RequestTileClick(int x, int y)
    {
        if (!TryGetPreparationPhase(out var prepa))
        {
            Debug.LogWarning("[PREPA] Impossible de trouver la phase de préparation.");
            return;
        }

        if (prepa != null && prepa.isActiveAndEnabled)
        {
            prepa.TryMoveEntityToTile(gameObject, x, y);
            Debug.Log($"[LOCAL] {gameObject.name} a cliqué sur la case ({x}, {y}).");
        }
    }

    private void HandleTileClicked(SetupTile setup)
    {
        TryCacheStats();
        if (ignoreClicksWhenReady && stats != null && stats.isReady) return;

        if (!TryGetPreparationPhase(out var prepa)) return;
        if (prepa == null || !prepa.isActiveAndEnabled) return;

        // Se déplacer uniquement si équipe verte (joueur)
        if (stats != null && stats.team != 0) return;

        prepa.TryMoveEntityToTile(gameObject, setup.tileX, setup.tileY);
        Debug.Log($"[PREPA] {gameObject.name} → case {{{setup.tileX},{setup.tileY}}}");
    }

    public void Ready()
    {
        TryCacheStats();
        if (stats == null)
        {
            Debug.LogWarning("[PREPA] Pas de Entity_StatistiqueCombat sur le joueur.");
            return;
        }

        stats.isReady = true;
        Debug.Log($"[PREPA] {gameObject.name} est PRÊT (isReady = true)");
        // La coroutine de Phase_PreparationPlacementCombat avancera toute seule quand tout le monde est prêt.
    }

    private bool TryGetPreparationPhase(out Phase_PreparationPlacementCombat phase)
    {
        phase = null;
        if (combatManager == null) return false;
        phase = combatManager.phasePrepa;
        return phase != null;
    }
}
