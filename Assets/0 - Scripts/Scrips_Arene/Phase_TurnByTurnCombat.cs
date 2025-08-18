using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat (Local)")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    private Combat_PhaseManager manager;

    public void InitPhase(Combat_PhaseManager combatManager)
    {
        manager = combatManager;

        Debug.Log($"[Phase_TurnByTurn] Début de la phase tour par tour sur l’arène {manager.arenaIndex}");

        // Couper le script de prépa si besoin
        if (manager.phasePrepa != null)
            manager.phasePrepa.enabled = false;

        if (manager.phaseEnter != null && manager.phaseEnter.AllFighters != null)
        {
            foreach (GameObject entity in manager.phaseEnter.AllFighters)
            {
                if (entity == null) continue;

                // ➜ Joueur : mode Combat
                var sm = entity.GetComponent<Player_ScriptManager>();
                if (sm) sm.SetTurnByTurnCombat();

                // Stats : reset "ready" + reset de tour
                if (entity.TryGetComponent(out Entity_StatistiqueCombat stats))
                {
                    if (stats.isReady) stats.isReady = false;
                    stats.ResetTurnStats();
                }
            }
        }

        ApplyCheckerboardToTiles();
    }

    private void ApplyCheckerboardToTiles()
    {
        if (manager == null || manager.tileGrid == null)
        {
            Debug.LogWarning("[Phase_TurnByTurn] tileGrid manquant, damier non appliqué.");
            return;
        }

        List<GameObject> allTiles = manager.tileGrid.GetAllTiles();
        if (allTiles == null || allTiles.Count == 0) return;

        foreach (GameObject tileObj in allTiles)
        {
            if (tileObj == null) continue;
            if (!tileObj.TryGetComponent(out SetupTile setup)) continue;

            setup.currentState = Tile_State.None;
            setup.isFighterActif = false;
        }

        Debug.Log("[Phase_TurnByTurn] Damier visuel ré-appliqué sur la grille (état None).");
    }
}
