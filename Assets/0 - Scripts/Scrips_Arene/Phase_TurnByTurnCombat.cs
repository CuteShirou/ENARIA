using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat (Local)")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    private Combat_PhaseManager manager;

    // Simulation simple de tour
    private int turnIndex = -1;
    private Timeline_CombatUI timeline;
    private SetupTile lastActiveTile = null;

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

        // 3) Appliquer état neutre aux tuiles (damier logique côté data)
        ApplyCheckerboardToTiles();

        // 4) UI Timeline
        timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);

        if (manager.phaseEnter?.AllFighters != null && manager.phaseEnter.AllFighters.Count > 0)
        {
            turnIndex = 0;
            var first = manager.phaseEnter.AllFighters[turnIndex];

            if (timeline)
            {
                timeline.SetCurrentEntity(first);
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

        // Raccourcis de test existants
        if (Input.GetKeyDown(KeyCode.Space)) { GoToNextFighter(); }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetPA(s.currentPA - 1);
                s.SetPM(s.currentPM - 1);
                RefreshUIForCurrent(e, false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetHP(s.currentHP - 10);
                RefreshUIForCurrent(e, true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            var e = GetCurrentEntity();
            if (!e) return;
            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetResForce(s.currentResistanceForce - 5f);
                s.SetResDex(s.currentResistanceDexterite - 5f);
                s.SetResMagie(s.currentResistanceMagie - 5f);
                s.SetResFoi(s.currentResistanceFoi - 5f);
                RefreshUIForCurrent(e, false);
            }
        }

        // --- Touches de debug pour fin de combat ---
        if (Input.GetKeyDown(KeyCode.O))
        {
            // met à 0 les PV de toute l'équipe VERTE via phaseEnter.greenTeam
            KillTeamForDebug(manager?.phaseEnter != null ? manager.phaseEnter.greenTeam : null);
            if (timeline) timeline.RefreshAllHP();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            // met à 0 les PV de toute l'équipe ROUGE via phaseEnter.redTeam
            KillTeamForDebug(manager?.phaseEnter != null ? manager.phaseEnter.redTeam : null);
            if (timeline) timeline.RefreshAllHP();
        }

        // Détection Win/Lose à chaque frame
        if (manager != null)
        {
            if (manager.TryEvaluateEndOfCombat())
            {
                // si fin détectée → Phase_End activée. Ce script sera désactivé.
                return;
            }
        }
    }

    // utilitaire debug pour "tuer" une équipe
    private void KillTeamForDebug(List<GameObject> team)
    {
        if (team == null) return;
        for (int i = 0; i < team.Count; i++)
        {
            var go = team[i];
            if (!go) continue;
            if (go.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetHP(0);
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
        if (!timeline) return;
        if (refreshHpInTimeline) timeline.RefreshAllHP();

        var panel = timeline.InfoPanel;
        if (panel && panel.gameObject.activeInHierarchy)
            panel.ShowFor(entity);
    }

    private void GoToNextFighter()
    {
        var fighters = manager.phaseEnter?.AllFighters;
        if (fighters == null || fighters.Count == 0) return;

        turnIndex = (turnIndex + 1) % fighters.Count;
        var current = fighters[turnIndex];

        if (timeline) timeline.SetCurrentEntity(current);
        HighlightEntityTile(current);
        RefreshUIForCurrent(current, true);
    }

    private void HighlightEntityTile(GameObject entity)
    {
        if (manager?.tileGrid == null || entity == null) return;

        if (lastActiveTile != null) lastActiveTile.isFighterActif = false;

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
            setup.isFighterActif = false;
        }

        Debug.Log("[Phase_TurnByTurn] Damier logique réinitialisé (état None).");
    }
}
