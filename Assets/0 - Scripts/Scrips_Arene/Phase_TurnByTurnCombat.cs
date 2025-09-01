using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Phase - Turn By Turn Combat")]
public class Phase_TurnByTurnCombat : MonoBehaviour
{
    public Combat_PhaseManager manager; //   Référence vers le manager de phases

    //   Index courant dans l'ordre d'initiative
    private int turnIndex = -1;

    //   Surbrillance de la tuile de l’entité active
    private SetupTile lastActiveTile = null;

    //   (Optionnel) Timeline si présente dans la scène
    private Timeline_CombatUI timeline;

    //   Tuile actuellement connue pour l'entité active (pour détecter les changements en déplacement)
    private GameObject lastTileForCurrent = null;

    //   Morts déjà traités (évite un double-traitement)
    private readonly HashSet<GameObject> removedDead = new();

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

        //   Passe tous les joueurs en mode combat + init début de phase
        for (int i = 0; i < manager.phaseEnter.AllFighters.Count; i++)
        {
            GameObject e = manager.phaseEnter.AllFighters[i];
            if (!e) continue;

            var sm = e.GetComponent<Player_ScriptManager>();
            if (sm != null) sm.SetTurnByTurnCombat();

            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.isReady = false;
                //   IMPORTANT : on ne remet PAS les PA/PM ici.
                // On suppose que l'entrée en phase a déjà mis les "current" égaux aux "base".
            }
        }

        //   Damier logique neutre côté data
        ApplyCheckerboardToTiles();

        //   Timeline (optionnel)
        timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);

        //   Premier tour
        turnIndex = manager.phaseEnter.AllFighters.Count > 0 ? 0 : -1;
        if (turnIndex >= 0) StartTurnForCurrent();
    }

    private void Update()
    {
        if (!isActiveAndEnabled) return;
        if (manager == null || manager.phaseEnter == null || manager.phaseEnter.AllFighters == null) return;
        if (turnIndex < 0 || turnIndex >= manager.phaseEnter.AllFighters.Count) return;

        //   Raccourcis debug / test
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

        //   Tient à jour mapping + surbrillance de l’entité active quand elle bouge
        TrackActiveEntityTileChange();

        //   Nouveau : traite les "morts" (libère case + retire du tour)
        ProcessDeathsIfAny();

        //   Détection de fin de combat centralisée dans le manager
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

        //   Fin de tour : reset PA/PM + décrémente la durée des effets (nouvelle logique)
        if (current && current.TryGetComponent(out Entity_StatistiqueCombat sEnd))
        {
            sEnd.ResetTurnStats();          //   on remonte PA/PM maintenant
            sEnd.TickActiveEffectsAtTurnEnd(); //   on consomme 1 tour de durée
        }

        //   Enchaîner sur le suivant
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

        //   IMPORTANT : on NE reset plus PA/PM ici.
        //   Applique les effets temporisés pour CE tour (ex: –10 PA, –10 PM, etc.)
        if (current && current.TryGetComponent(out Entity_StatistiqueCombat sTurn))
            sTurn.ApplyActiveEffectsAtTurnStart();

        if (current && current.TryGetComponent(out Entity_SkillCaster caster))
            caster.ResetSkillTurnUsage(); //   quotas de sorts (par cible) remis à zéro au début du tour

        if (timeline)
        {
            timeline.SetCurrentEntity(current);
            timeline.RefreshAllHP();
        }

        //   Assure la cohérence initiale (mapping + surbrillance) en début de tour
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
    // ============   LOGIQUE : suivi de déplacement actif   ===============
    // =====================================================================

    /// <summary>
    ///   Appelée chaque frame : si l'entité active change de tuile (en se déplaçant),
    /// met à jour les dictionnaires de placement + l'InfoTile + la surbrillance.
    /// </summary>
    private void TrackActiveEntityTileChange()
    {
        if (manager == null || manager.tileGrid == null) return;

        var current = GetCurrentEntity();
        if (current == null) return;

        //   Détermine la tuile la plus proche de la position actuelle (robuste quand les dicos ne sont pas à jour)
        GameObject nearest = FindNearestTileTo(current.transform.position);
        if (!nearest) return;

        //   Si on a changé de tuile → on met à jour les structures et la surbrillance
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
    ///   En début de tour, assure la cohérence des dicos et de la surbrillance
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
    ///   Renvoie la tuile la plus proche d'une position monde.
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

            float d = (t.transform.position - worldPos).sqrMagnitude; //   sqrDist = plus léger
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    // ---------------------------------------------------------------------
    // Gestion unifiée des morts (libère la case + retire du tour)
    private void ProcessDeathsIfAny()
    {
        var list = manager.phaseEnter.AllFighters;
        if (list == null || list.Count == 0) return;

        var snapshot = new List<GameObject>(list);

        for (int i = 0; i < snapshot.Count; i++)
        {
            var e = snapshot[i];
            if (!e || removedDead.Contains(e)) continue;

            if (!e.TryGetComponent(out Entity_StatistiqueCombat s)) continue;

            if (s.currentHP <= 0)
                HandleEntityDeath(e, s);
        }
    }

    /// <summary>
    ///   Libère la case, cache visuel, retire l'entité de AllFighters, et met la timeline à jour.
    /// </summary>
    private void HandleEntityDeath(GameObject entity, Entity_StatistiqueCombat stats)
    {
        removedDead.Add(entity);

        stats.isDead = true;

        var tile = manager.tileGrid.GetTileOfEntity(entity);
        if (tile && tile.TryGetComponent(out InfoTile ti)) ti.SetFree();
        manager.tileGrid.UnregisterEntity(entity);

        foreach (var r in entity.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        foreach (var c in entity.GetComponentsInChildren<Collider>(true)) c.enabled = false;

        var pc = entity.GetComponent<Player_CombatController>(); if (pc) pc.enabled = false;
        var mc = entity.GetComponent<Monster_CombatController>(); if (mc) mc.enabled = false;

        RemoveFromInitiative(entity);

        if (timeline) timeline.RefreshAllHP();
    }

    /// <summary>
    ///   Retire l'entité de AllFighters en ajustant turnIndex proprement.
    /// </summary>
    private void RemoveFromInitiative(GameObject entity)
    {
        var list = manager.phaseEnter.AllFighters;
        if (list == null) return;

        int idx = list.IndexOf(entity);
        if (idx < 0) return;

        bool wasCurrent = (turnIndex == idx);
        list.RemoveAt(idx);

        if (list.Count == 0)
        {
            turnIndex = -1;
            return;
        }

        if (wasCurrent)
        {
            turnIndex = (idx >= list.Count) ? 0 : idx;
            StartTurnForCurrent();
        }
        else if (idx < turnIndex)
        {
            turnIndex = Mathf.Max(0, turnIndex - 1);
        }
    }

    //   Appelé par le caster après un sort : force la mise à jour des PV dans la Timeline
    public void RefreshTimelineHP()
    {
        if (timeline) timeline.RefreshAllHP();
    }
}
