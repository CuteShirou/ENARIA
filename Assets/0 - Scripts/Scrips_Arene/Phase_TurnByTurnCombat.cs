using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat (Local)")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    private Combat_PhaseManager manager;

    //------------------------------------------------------------
    // Appelée par le manager quand on passe en phase TourParTour
    public void InitPhase(Combat_PhaseManager combatManager)
    {
        manager = combatManager;

        Debug.Log($"[Phase_TurnByTurn] Début de la phase tour par tour sur l’arène {manager.arenaIndex}");

        // 1) Désactive la phase de préparation
        if (manager.phasePrepa != null)
            manager.phasePrepa.enabled = false;

        // 2) Active/désactive les bons contrôleurs + reset de tour (optionnel)
        if (manager.phaseEnter != null && manager.phaseEnter.AllFighters != null)
        {
            foreach (GameObject entity in manager.phaseEnter.AllFighters)
            {
                if (entity == null) continue;

                var prepCtrl = entity.GetComponent<Player_ControllerPhasePreparation>();
                if (prepCtrl != null) prepCtrl.enabled = false;

                var turnCtrl = entity.GetComponent<Player_ControllerPhaseTurnByTurn>();
                if (turnCtrl != null) turnCtrl.enabled = true;

                // Option pratique : reset PA/PM en début de phase
                if (entity.TryGetComponent(out Entity_StatistiqueCombat stats))
                    stats.ResetTurnStats();
            }
        }

        // 3) Appliquer le damier à toutes les cases (état logique = None)
        ApplyCheckerboardToTiles();
    }

    //------------------------------------------------------------
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

            // Réinitialise l'état logique : le visuel damier est géré par Tile_Visual quand l'état est None
            setup.currentState = Tile_State.None;
            setup.isFighterActif = false; // par sécurité, retire l'indicateur d'actif si tu l'utilises
        }

        Debug.Log("[Phase_TurnByTurn] Damier visuel ré-appliqué sur la grille (état None).");
    }
}
