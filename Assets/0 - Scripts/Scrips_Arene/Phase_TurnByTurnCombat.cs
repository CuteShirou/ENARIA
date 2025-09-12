using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase_TurnByTurnCombat : MonoBehaviour
{
    public Combat_PhaseManager manager; // Référence vers le manager de phases

    // Index courant dans l'ordre d'initiative
    private int turnIndex = -1;

    // Surbrillance de la tuile de l’entité active
    private SetupTile lastActiveTile = null;

    // (Optionnel) Timeline si présente dans la scène
    private Timeline_CombatUI timeline;

    // Tuile actuellement connue pour l'entité active (pour détecter les changements en déplacement)
    private GameObject lastTileForCurrent = null;

    // Morts déjà traités (évite un double-traitement)
    private readonly HashSet<GameObject> removedDead = new();

    [Header("Hauteur Y en TourParTour")]
    [SerializeField] private bool clampYDuringTurn = true; // Applique la hauteur cible seulement si l'entité est immobile
    [SerializeField] private float greenTeamY = 4.3f;      // Hauteur des joueurs (équipe Verte)
    [SerializeField] private float redTeamY = 3.8f;        // Hauteur des monstres (équipe Rouge)

    [Header("Mort : animation")]
    [SerializeField] private string deathStateTag = "Death";   // Tag ou nom du state de mort
    [SerializeField] private float deathDetectTimeout = 1.0f;  // Temps max pour détecter l'entrée dans Death
    [SerializeField] private float deathFallbackTimeout = 0.5f; // Durée de secours si le state n'est jamais détecté
    [SerializeField, Range(0f, 0.1f)]
    private float deathEndEpsilon = 0.01f;                     // Marge pour sortir dès la fin du state
    [SerializeField] private bool deactivateRootOnDeath = true; // Si true, on désactive la racine après les pop-ups


    // Appelée par les contrôleurs pour construire une cible XZ vers une tuile donnée
    public Vector3 GetMoveTargetForTileXZ(GameObject entity, SetupTile setup)
    {
        // Renvoie la cible en XZ (Y conservé depuis l'entité), ou la position actuelle si refs manquantes
        if (manager == null || manager.tileGrid == null || entity == null || setup == null)
            return entity != null ? entity.transform.position : Vector3.zero;

        return manager.tileGrid.GetTileXZTargetForEntity(entity, setup.gameObject);
    }

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

        // Passe tous les combattants en mode tour par tour
        for (int i = 0; i < manager.phaseEnter.AllFighters.Count; i++)
        {
            GameObject e = manager.phaseEnter.AllFighters[i];
            if (!e) continue;

            var sm = e.GetComponent<Player_ScriptManager>();
            if (sm != null) sm.SetTurnByTurnCombat();

            if (e.TryGetComponent(out Entity_StatistiqueCombat s))
            {
                s.isReady = false; // Pas de reset PA/PM ici
            }

            // Harmonise la hauteur Y dès l'entrée en phase (uniquement si l'entité est immobile)
            if (clampYDuringTurn) ClampYFor(e);
        }

        // Damier logique neutre côté data
        ApplyCheckerboardToTiles();

        // Timeline (optionnel)
        timeline = FindAnyObjectByType<Timeline_CombatUI>(FindObjectsInactive.Include);

        // Premier tour
        turnIndex = manager.phaseEnter.AllFighters.Count > 0 ? 0 : -1;
        if (turnIndex >= 0) StartTurnForCurrent();
    }

    private void Update()
    {
        if (!isActiveAndEnabled) return;
        if (manager == null || manager.phaseEnter == null || manager.phaseEnter.AllFighters == null) return;
        if (turnIndex < 0 || turnIndex >= manager.phaseEnter.AllFighters.Count) return;

        // Harmonise la hauteur Y de l'entité active sans interrompre un déplacement en cours
        if (clampYDuringTurn)
        {
            var current = GetCurrentEntity();
            if (current) ClampYFor(current);
        }

        // Raccourcis debug / test
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

        // Tient à jour mapping + surbrillance de l’entité active quand elle bouge
        TrackActiveEntityTileChange();

        // Traite les morts (libère case + retire du tour)
        ProcessDeathsIfAny();

        // Détection de fin de combat centralisée dans le manager
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

        // Fin de tour : reset PA/PM + décrémente la durée des effets
        if (current && current.TryGetComponent(out Entity_StatistiqueCombat sEnd))
        {
            sEnd.ResetTurnStats();
            sEnd.TickActiveEffectsAtTurnEnd();
        }

        // Enchaîner sur le suivant
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

        // Applique les effets temporisés pour CE tour
        if (current && current.TryGetComponent(out Entity_StatistiqueCombat sTurn))
            sTurn.ApplyActiveEffectsAtTurnStart();

        if (current && current.TryGetComponent(out Entity_SkillCaster caster))
            caster.ResetSkillTurnUsage(); // quotas de sorts réinitialisés

        if (timeline)
        {
            timeline.SetCurrentEntity(current);
            timeline.RefreshAllHP();
        }

        // Assure la cohérence initiale (mapping + surbrillance) en début de tour
        EnsureCurrentEntityMappingAndHighlight(current);

        // Harmonise la hauteur Y du combattant actif en début de tour (si immobile)
        if (clampYDuringTurn) ClampYFor(current);

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

    private void TrackActiveEntityTileChange()
    {
        if (manager == null || manager.tileGrid == null) return;

        var current = GetCurrentEntity();
        if (current == null) return;

        GameObject nearest = FindNearestTileTo(current.transform.position);
        if (!nearest) return;

        if (lastTileForCurrent != nearest)
        {
            var prev = manager.tileGrid.GetTileOfEntity(current);
            if (prev && prev.TryGetComponent(out InfoTile prevInfo)) prevInfo.SetFree();

            manager.tileGrid.RegisterEntity(current, nearest);
            if (nearest.TryGetComponent(out InfoTile newInfo)) newInfo.SetOccupied();

            HighlightEntityTile(current);

            if (clampYDuringTurn) ClampYFor(current);

            lastTileForCurrent = nearest;
        }
    }

    private void EnsureCurrentEntityMappingAndHighlight(GameObject current)
    {
        if (current == null || manager == null || manager.tileGrid == null) return;

        GameObject nearest = FindNearestTileTo(current.transform.position);
        if (!nearest) return;

        GameObject mapped = manager.tileGrid.GetTileOfEntity(current);

        if (mapped != nearest)
        {
            if (mapped && mapped.TryGetComponent(out InfoTile oldInfo)) oldInfo.SetFree();
            manager.tileGrid.RegisterEntity(current, nearest);
            if (nearest.TryGetComponent(out InfoTile newInfo)) newInfo.SetOccupied();
        }

        lastTileForCurrent = nearest;

        HighlightEntityTile(current);

        if (clampYDuringTurn) ClampYFor(current);
    }

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

            float d = (t.transform.position - worldPos).sqrMagnitude;
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

    // Libère la case, joue l'animation de mort, masque le visuel à la fin, puis désactive la racine après pop-ups.
    private void HandleEntityDeath(GameObject entity, Entity_StatistiqueCombat stats)
    {
        removedDead.Add(entity);
        stats.isDead = true;

        // Libère la tuile et les dicos
        var tile = manager.tileGrid.GetTileOfEntity(entity);
        if (tile && tile.TryGetComponent(out InfoTile ti)) ti.SetFree();
        manager.tileGrid.UnregisterEntity(entity);

        // Coupe les contrôleurs de déplacement/IA pour éviter toute action post-mortem
        var pc = entity.GetComponent<Player_CombatController>(); if (pc) pc.enabled = false;
        var mc = entity.GetComponent<Monster_CombatController>(); if (mc) mc.enabled = false;

        // Désactive les colliders immédiatement (laisse le visuel actif pour l'animation de mort)
        foreach (var c in entity.GetComponentsInChildren<Collider>(true)) c.enabled = false;

        // Lance la séquence de mort : animation → hide visuel → désactivation racine après popups
        StartCoroutine(Co_PlayDeathThenHideVisual(entity));

        if (timeline) timeline.RefreshAllHP();
    }

    // Joue l'anim "Death" via Entity_Animations, attend la fin, puis masque les renderers et passe à la désactivation root
    private IEnumerator Co_PlayDeathThenHideVisual(GameObject entity)
    {
        if (!entity) yield break;

        // Récupère le contrôleur d'animations (pour PlayDeath)
        var anim = entity.GetComponent<Entity_Animations>();

        // Déclenche l'animation de mort si possible
        if (anim != null && anim.isActiveAndEnabled)
        {
            anim.PlayDeath(); // Lance l'animation "Death"
            yield return WaitForDeathAnimation(entity, deathFallbackTimeout); // Attend la fin exacte, sinon petit fallback

        }
        else
        {
            // Si pas d'anim disponible, micro-pause pour éviter un "cut" trop sec
            yield return new WaitForSeconds(0.05f);
        }

        // Masque le visuel : coupe tous les Renderers (le son/FX restent possibles)
        foreach (var r in entity.GetComponentsInChildren<Renderer>(true)) r.enabled = false;

        // Termine le processus (désactivation racine après pop-ups) ou retire directement de l'initiative
        if (deactivateRootOnDeath)
            yield return Co_FinalizeDeathAfterPopups(entity);
        else
            RemoveFromInitiative(entity);
    }

    // Attend la fin de l'animation de mort sur l'Animator
    // Attend précisément la fin du state "Death"
    // - On attend d'abord que l'Animator ENTRE dans le state Death (timeout court).
    // - Puis on sort dès que ce MÊME state est terminé (normalizedTime >= 1f - epsilon et hors transition).
    private IEnumerator WaitForDeathAnimation(GameObject entity, float fallbackTimeoutSeconds)
    {
        var animator = entity ? entity.GetComponentInChildren<Animator>(true) : null;
        if (animator == null)
        {
            // Pas d'Animator : on ne peut pas synchroniser -> petit fallback minimal
            yield return new WaitForSeconds(0.05f);
            yield break;
        }

        // Laisse 1 frame pour que PlayDeath() déclenche le state
        yield return null;

        // 1) Attendre l'entrée dans le state/tag "Death" (timeout court)
        bool enteredDeath = false;
        int deathStateHash = 0;
        float tDetect = 0f;

        while (tDetect < Mathf.Max(0.02f, deathDetectTimeout))
        {
            if (!animator.isActiveAndEnabled) break;

            var st = animator.GetCurrentAnimatorStateInfo(0);

            // Correspondance par TAG ou par NOM (au choix selon ton Animator)
            if (st.IsTag(deathStateTag) || st.IsName(deathStateTag))
            {
                enteredDeath = true;
                deathStateHash = st.fullPathHash; // on capture le state exact
                break;
            }

            tDetect += Time.deltaTime;
            yield return null;
        }

        if (!enteredDeath)
        {
            // Si on n'a pas détecté l'entrée du state Death, on évite d'attendre trop longtemps
            yield return new WaitForSeconds(Mathf.Min(0.2f, fallbackTimeoutSeconds));
            yield break;
        }

        while (true)
        {
            if (!animator.isActiveAndEnabled) break;

            var st = animator.GetCurrentAnimatorStateInfo(0);

            // Si on a quitté ce state (changement de hash), on considère la mort jouée
            if (st.fullPathHash != deathStateHash)
                break;

            if (!animator.IsInTransition(0) && st.normalizedTime >= (1f - deathEndEpsilon))
                break;

            yield return null;
        }
    }


    // attend la fin des pop-ups de l'entité puis désactive la racine et retire de l'initiative
    private System.Collections.IEnumerator Co_FinalizeDeathAfterPopups(GameObject entity)
    {
        Popup_DisplayNumber pop = entity ? entity.GetComponent<Popup_DisplayNumber>() : null;

        if (pop != null)
            yield return pop.WaitMyPopupsToFinish();
        else
            yield return null;

        if (entity != null) entity.SetActive(false);

        RemoveFromInitiative(entity);
    }

    // Retire l'entité de AllFighters en ajustant turnIndex proprement.
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

    // Appelé par le caster après un sort : force la mise à jour des PV dans la Timeline
    public void RefreshTimelineHP()
    {
        if (timeline) timeline.RefreshAllHP();
    }

    // =====================================================================
    // ============   UTILITAIRES : gestion de la hauteur Y   ==============
    // =====================================================================

    private float GetTeamY(int team)
    {
        return (team == 0) ? greenTeamY : redTeamY;
    }

    private void ClampYFor(GameObject entity)
    {
        if (!entity) return;

        if (IsEntityMoving(entity)) return;

        if (!entity.TryGetComponent(out Entity_StatistiqueCombat s)) return;

        Vector3 p = entity.transform.position;
        float targetY = GetTeamY(s.team);

        if (!Mathf.Approximately(p.y, targetY))
        {
            p.y = targetY;
            entity.transform.position = p;
        }
    }
}
