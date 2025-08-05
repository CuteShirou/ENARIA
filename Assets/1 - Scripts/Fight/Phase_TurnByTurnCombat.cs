using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Phase_TurnByTurnCombat : MonoBehaviour
{
    private Combat_PhaseManager manager;

    //------------------------------------------------------------
    public void InitPhase(Combat_PhaseManager combatManager)
    {
        manager = combatManager;

        Debug.Log($"[Phase_TurnByTurn] Début de la phase de combat en tour par tour sur l’arène {manager.arenaIndex}");

        // 1. Désactive Phase_PreparationPlacementCombat
        manager.phasePrepa.enabled = false;

        // 2. Active/désactive les bons contrôleurs
        foreach (GameObject entity in manager.phaseEnter.AllFighters)
        {
            if (entity == null) continue;

            var prepCtrl = entity.GetComponent<Player_ControllerPhasePreparation>();
            if (prepCtrl != null) prepCtrl.enabled = false;

            var turnCtrl = entity.GetComponent<Player_ControllerPhaseTurnByTurn>();
            if (turnCtrl != null) turnCtrl.enabled = true;
        }

        // 3. Appliquer un damier visuel à toutes les cases
        if (NetworkServer.active)
        {
            ApplyCheckerboardToTiles();
        }
    }

    //------------------------------------------------------------
    [Server]
    private void ApplyCheckerboardToTiles()
    {
        List<GameObject> allTiles = manager.tileGrid.GetAllTiles();

        foreach (GameObject tileObj in allTiles)
        {
            if (tileObj == null) continue;

            if (!tileObj.TryGetComponent(out Setup_NetworkTile setup)) continue;

            int x = setup.tileX;
            int y = setup.tileY;

            // Réinitialise l'état logique
            setup.currentState = TileState.None;

            // Le visuel damier sera appliqué automatiquement côté client dans Tile_ClientVisual
            // en fonction de tileX + tileY (voir étape 2)
        }

        Debug.Log("[Phase_TurnByTurn] Damier visuel appliqué sur la grille.");
    }
}
