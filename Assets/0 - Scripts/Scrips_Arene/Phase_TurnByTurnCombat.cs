using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    public Combat_PhaseManager manager; // [FR] Référence vers le manager de phases

    // [FR] Index courant dans l'ordre d'initiative
    private int turnIndex = -1;

    // [FR] Surbrillance de la tuile de l’entité active
    private SetupTile lastActiveTile = null;

    // [FR] (Optionnel) Timeline si présente dans la scène
    private Timeline_CombatUI timeline;

    // [FR] Tuile actuellement connue pour l'entité active (pour détecter les changements en déplacement)
    private GameObject lastTileForCurrent = null;

    // Constructeur / Déconstructeur
    public Phase_TurnByTurnCombat() { }
    ~Phase_TurnByTurnCombat() { }

    // ---------------------------------------------------------------------
    // InitPhase : appelée par Combat_PhaseManager au passage en phase TurnByTurn
    public void InitPhase(Combat_PhaseManager combatManager)
    {
        manager = combatManager;

        if (manager == null || manager.phaseEnter == null || manager.phaseEnter.AllFighters == null)
        {
            Debug.LogError("[TurnByTurn] Manager/Initiative manquants.");
            enabled = false;
            return;
        }

        // [FR] Passe tous les joueurs en mode combat + reset début de phase
        for (int i = 0; i < manager.phaseEnter.AllFighters.Count; i++)
        {
            GameObject e = manager.phaseEnter.AllFighters[i];
            if (!e) continue;

            var sm = e.GetComponent<Player_ScriptManager>();
            if (sm != null) sm.SetTurnByTurnCombat();

            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.isReady = false;
                s.ResetTurnStats();
            }
        }

        // [FR] Damier logique neutre côté data
        ApplyCheckerboardToTiles();

        // [FR] Timeline (optionnel)
        timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);

        // [FR] Premier tour
        turnIndex = manager.phaseEnter.AllFighters.Count > 0 ? 0 : -1;
        if (turnIndex >= 0) StartTurnForCurrent();
    }

    private void Update()
    {
        if (!isActiveAndEnabled) return;
        if (manager == null || manager.phaseEnter == null || manager.phaseEnter.AllFighters == null) return;
        if (turnIndex < 0 || turnIndex >= manager.phaseEnter.AllFighters.Count) return;

        // [FR] Raccourcis debug / test
        if (Input.GetKeyDown(KeyCode.Space)) EndTurn();
        else if (Input.GetKeyDown(KeyCode.X))
        {
            var e = GetCurrentEntity();
            if (e && e.TryGetComponent(out Entity_StatistiqueCombat s))
            { s.SetPA(s.currentPA - 1); s.SetPM(s.currentPM - 1); RefreshUIForCurrent(e, false); }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            var e = GetCurrentEntity();
            if (e && e.TryGetComponent(out Entity_StatistiqueCombat s))
            { s.SetHP(s.currentHP - 10); RefreshUIForCurrent(e, true); }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            var e = GetCurrentEntity();
            if (e && e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.SetResForce(s.currentResistanceForce - 5f);
                s.SetResDex(s.currentResistanceDexterite - 5f);
                s.SetResMagie(s.currentResistanceMagie - 5f);
                s.SetResFoi(s.currentResistanceFoi - 5f);
                RefreshUIForCurrent(e, false);
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            KillTeamForDebug(manager.phaseEnter.greenTeam);
            if (timeline) timeline.RefreshAllHP();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            KillTeamForDebug(manager.phaseEnter.redTeam);
            if (timeline) timeline.RefreshAllHP();
        }

        // [FR] TIENT À JOUR en temps réel : placement + surbrillance quand l'entité active se déplace
        TrackActiveEntityTileChange();

        // [FR] Détection de fin de combat centralisée dans le manager
        if (manager.TryEvaluateEndOfCombat()) return;
    }

    // ---------------------------------------------------------------------
    // IsMyTurn : exposé pour que les scripts vérifient s’ils peuvent agir
    public bool IsMyTurn(GameObject entity)
    {
        if (entity == null) return false;
        if (manager == null || manager.phaseEnter == null) return false;
        var list = manager.phaseEnter.AllFighters;
        if (list == null || list.Count == 0) return false;
        if (turnIndex < 0 || turnIndex >= list.Count) return false;
        return list[turnIndex] == entity;
    }

    // ---------------------------------------------------------------------
    // EndTurn : termine le tour courant si pas de déplacement en cours
    public void EndTurn()
    {
        var list = manager.phaseEnter.AllFighters;
        if (list == null || list.Count == 0) return;

        var current = list[turnIndex];
        if (current && IsEntityMoving(current))
        {
            Debug.Log("[Turn] Fin refusée: entité en mouvement.");
            return;
        }

        turnIndex = (turnIndex + 1) % list.Count;
        StartTurnForCurrent();
    }

    // ---------------------------------------------------------------------
    // StartTurnForCurrent : initialise le tour de l’entité à turnIndex
    private void StartTurnForCurrent()
    {
        var list = manager.phaseEnter.AllFighters;
        if (list == null || list.Count == 0) return;

        var current = list[turnIndex];

        if (current && current.TryGetComponent(out Entity_StatistiqueCombat s))
            s.ResetTurnStats();

        if (current && current.TryGetComponent(out Entity_SkillCaster caster))
            caster.ResetSkillTurnUsage();

        if (timeline)
        {
            timeline.SetCurrentEntity(current);
            timeline.RefreshAllHP();
        }

        // [FR] Assure la cohérence initiale (mapping + surbrillance) en début de tour
        EnsureCurrentEntityMappingAndHighlight(current);

        Debug.Log($"[Turn] Début → {current?.name} (index {turnIndex})");
    }

    // ---------------------------------------------------------------------
    // IsEntityMoving : cherche un bool public/propriété "isMoving" sur l’entité
    private bool IsEntityMoving(GameObject entity)
    {
        if (!entity) return false;
        var mbs = entity.GetComponents<MonoBehaviour>();
        for (int i = 0; i < mbs.Length; i++)
        {
            var t = mbs[i].GetType();
            var f = t.GetField("isMoving");
            if (f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(mbs[i])) return true;

            var p = t.GetProperty("isMoving");
            if (p != null && p.PropertyType == typeof(bool) && (bool)p.GetValue(mbs[i], null)) return true;
        }
        return false;
    }

    // ---------------------------------------------------------------------
    // Utilitaires UI & Grille
    private GameObject GetCurrentEntity()
    {
        if (manager == null || manager.phaseEnter == null) return null;
        var list = manager.phaseEnter.AllFighters;
        if (list == null || list.Count == 0) return null;
        if (turnIndex < 0 || turnIndex >= list.Count) return null;
        return list[turnIndex];
    }

    private void RefreshUIForCurrent(GameObject entity, bool refreshHp)
    {
        if (!timeline) return;
        if (refreshHp) timeline.RefreshAllHP();

        var panel = timeline.InfoPanel;
        if (panel && panel.gameObject.activeInHierarchy)
            panel.ShowFor(entity);
    }

    private void HighlightEntityTile(GameObject entity)
    {
        if (manager == null || manager.tileGrid == null || entity == null) return;

        if (lastActiveTile != null) lastActiveTile.isFighterActif = false;

        var tileObj = manager.tileGrid.GetTileOfEntity(entity);
        if (tileObj != null && tileObj.TryGetComponent(out SetupTile setup))
        {
            setup.isFighterActif = true;
            lastActiveTile = setup;
        }
        else lastActiveTile = null;
    }

    private void ApplyCheckerboardToTiles()
    {
        if (manager == null || manager.tileGrid == null) return;
        List<GameObject> tiles = manager.tileGrid.GetAllTiles();
        if (tiles == null) return;

        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            if (!t || !t.TryGetComponent(out SetupTile setup)) continue;
            setup.currentState = Tile_State.None;
            setup.isFighterActif = false;
        }
    }

    private void KillTeamForDebug(List<GameObject> team)
    {
        if (team == null) return;
        for (int i = 0; i < team.Count; i++)
        {
            var go = team[i];
            if (!go) continue;
            if (go.TryGetComponent(out Entity_StatistiqueCombat s)) s.SetHP(0);
        }
    }

    // =====================================================================
    // ============   NOUVELLE LOGIQUE : suivi de déplacement   =============
    // =====================================================================

    /// <summary>
    /// [FR] Appelée chaque frame : si l'entité active change de tuile (en se déplaçant),
    /// met à jour les dictionnaires de placement + l'InfoTile + la surbrillance.
    /// </summary>
    private void TrackActiveEntityTileChange()
    {
        if (manager == null || manager.tileGrid == null) return;

        var current = GetCurrentEntity();
        if (current == null) return;

        // [FR] Détermine la tuile la plus proche de la position actuelle (robuste quand les dicos ne sont pas à jour)
        GameObject nearest = FindNearestTileTo(current.transform.position);
        if (!nearest) return;

        // [FR] Si on a changé de tuile → on met à jour les structures et la surbrillance
        if (lastTileForCurrent != nearest)
        {
            // Libère l'ancienne tuile (si connue)
            var prev = manager.tileGrid.GetTileOfEntity(current);
            if (prev && prev.TryGetComponent(out InfoTile prevInfo)) prevInfo.SetFree();

            // Enregistre la nouvelle (met à jour entityToTile/tileToEntity)
            manager.tileGrid.RegisterEntity(current, nearest);
            if (nearest.TryGetComponent(out InfoTile newInfo)) newInfo.SetOccupied();

            // Rafraîchit la surbrillance visuelle
            HighlightEntityTile(current);

            // Mémorise
            lastTileForCurrent = nearest;
        }
    }

    /// <summary>
    /// [FR] En début de tour, assure la cohérence des dicos et de la surbrillance
    /// avec la position réelle de l'entité active.
    /// </summary>
    private void EnsureCurrentEntityMappingAndHighlight(GameObject current)
    {
        if (current == null || manager == null || manager.tileGrid == null) return;

        // Tuile détectée par proximité (source de vérité)
        GameObject nearest = FindNearestTileTo(current.transform.position);
        if (!nearest) return;

        // Tuile référencée dans les dicos (peut être obsolète)
        GameObject mapped = manager.tileGrid.GetTileOfEntity(current);

        if (mapped != nearest)
        {
            if (mapped && mapped.TryGetComponent(out InfoTile oldInfo)) oldInfo.SetFree();
            manager.tileGrid.RegisterEntity(current, nearest);
            if (nearest.TryGetComponent(out InfoTile newInfo)) newInfo.SetOccupied();
        }

        lastTileForCurrent = nearest;

        // Surbrillance
        HighlightEntityTile(current);
    }

    /// <summary>
    /// [FR] Renvoie la tuile la plus proche d'une position monde.
    /// </summary>
    private GameObject FindNearestTileTo(Vector3 worldPos)
    {
        if (manager == null || manager.tileGrid == null) return null;

        var tiles = manager.tileGrid.GetAllTiles();
        if (tiles == null || tiles.Count == 0) return null;

        GameObject best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            if (!t) continue;

            float d = (t.transform.position - worldPos).sqrMagnitude; // [FR] sqrDist = plus léger
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    // [FR] Appelé par le caster après un sort : force la mise à jour des PV dans la Timeline
    public void RefreshTimelineHP()
    {
        // [FR] Met à jour immédiatement la barre de vie de toutes les entités dans la Timeline
        if (timeline) timeline.RefreshAllHP();
    }

}
