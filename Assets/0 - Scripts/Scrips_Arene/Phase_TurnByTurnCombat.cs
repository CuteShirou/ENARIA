using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat (Local)")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    private Combat_PhaseManager manager;

    // --- Simulation simple de tour ---
    private int turnIndex = -1;                 // index courant dans AllFighters
    private Timeline_CombatUI timeline;         // ref UI
    private SetupTile lastActiveTile = null;    // pour éteindre l’ancien highlight

    public void InitPhase(Combat_PhaseManager combatManager)
    {
        manager = combatManager;

        Debug.Log($"[Phase_TurnByTurn] Début de la phase tour par tour sur l’arène {manager.arenaIndex}");

        // 1) Couper Prépa
        if (manager.phasePrepa) manager.phasePrepa.enabled = false;

        // 2) Activer contrôleurs + reset des stats + clear Ready
        if (manager.phaseEnter?.AllFighters != null)
        {
            foreach (GameObject entity in manager.phaseEnter.AllFighters)
            {
                if (!entity) continue;

                var sm = entity.GetComponent<Player_ScriptManager>();
                if (sm) sm.SetTurnByTurnCombat();

                if (entity.TryGetComponent(out Entity_StatistiqueCombat stats))
                {
                    if (stats.isReady) stats.isReady = false;
                    stats.ResetTurnStats();
                }
            }
        }

        // 3) Appliquer le damier logique (None)
        ApplyCheckerboardToTiles();

        // 4) Timeline : focus sur le premier combattant (si existant)
        timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);

        if (manager.phaseEnter?.AllFighters != null && manager.phaseEnter.AllFighters.Count > 0)
        {
            turnIndex = 0;
            var first = manager.phaseEnter.AllFighters[turnIndex];

            if (timeline)
            {
                timeline.SetCurrentEntity(first); // passe en prefab “Actif”
                timeline.RefreshAllHP();
            }

            HighlightEntityTile(first);
        }
        else
        {
            turnIndex = -1;
        }
    }

    private void Update()
    {
        if (!isActiveAndEnabled) return;
        if (turnIndex < 0) return;

        // --- TEST MANUEL ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextFighter();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            // -1 PA / -1 PM
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetPA(s.currentPA - 1);
                s.SetPM(s.currentPM - 1);
                RefreshUIForCurrent(e, refreshHpInTimeline: false); // pas besoin de maj HP ici
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            // -10 HP
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetHP(s.currentHP - 10);
                RefreshUIForCurrent(e, refreshHpInTimeline: true); // met à jour la barre HP
            }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            // -5% à toutes les résistances
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetResForce(s.currentResistanceForce - 5f);
                s.SetResDex(s.currentResistanceDexterite - 5f);
                s.SetResMagie(s.currentResistanceMagie - 5f);
                s.SetResFoi(s.currentResistanceFoi - 5f);
                RefreshUIForCurrent(e, refreshHpInTimeline: false); // pas de maj HP
            }
        }
    }

    private GameObject GetCurrentEntity()
    {
        var fighters = manager.phaseEnter?.AllFighters;
        if (fighters == null || fighters.Count == 0) return null;
        if (turnIndex < 0 || turnIndex >= fighters.Count) return null;
        return fighters[turnIndex];
    }

    private void RefreshUIForCurrent(GameObject entity, bool refreshHpInTimeline)
    {
        if (timeline)
        {
            if (refreshHpInTimeline) timeline.RefreshAllHP();

            // Réaffiche l'info-bulle pour recharger PA/PM/Res/HP à la volée
            var panel = timeline.InfoPanel; // exposé par Timeline_CombatUI
            if (panel && panel.gameObject.activeInHierarchy)
                panel.ShowFor(entity);
        }
    }

    private void GoToNextFighter()
    {
        var fighters = manager.phaseEnter?.AllFighters;
        if (fighters == null || fighters.Count == 0) return;

        turnIndex = (turnIndex + 1) % fighters.Count;
        var current = fighters[turnIndex];

        // Timeline → bascule l’item actif
        if (timeline) timeline.SetCurrentEntity(current);

        // Grille → highlight de la case
        HighlightEntityTile(current);

        // Réaffiche la bulle pour la nouvelle entité
        RefreshUIForCurrent(current, refreshHpInTimeline: true);
    }

    private void HighlightEntityTile(GameObject entity)
    {
        if (manager?.tileGrid == null || entity == null) return;

        // Éteindre l’ancienne tuile active
        if (lastActiveTile != null)
            lastActiveTile.isFighterActif = false;

        // Allumer la tuile de l’entité courante
        var tileObj = manager.tileGrid.GetTileOfEntity(entity);
        if (tileObj != null && tileObj.TryGetComponent(out SetupTile setup))
        {
            setup.isFighterActif = true;
            lastActiveTile = setup;
        }
        else
        {
            lastActiveTile = null;
        }
    }

    private void ApplyCheckerboardToTiles()
    {
        if (manager?.tileGrid == null)
        {
            Debug.LogWarning("[Phase_TurnByTurn] tileGrid manquant, damier non appliqué.");
            return;
        }

        List<GameObject> allTiles = manager.tileGrid.GetAllTiles();
        if (allTiles == null || allTiles.Count == 0) return;

        foreach (GameObject tileObj in allTiles)
        {
            if (!tileObj) continue;
            if (!tileObj.TryGetComponent(out SetupTile setup)) continue;

            setup.currentState = Tile_State.None;
            setup.isFighterActif = false; // on repart sans highlight
        }

        Debug.Log("[Phase_TurnByTurn] Damier logique réinitialisé (état None).");
    }
}
