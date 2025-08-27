using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [FR] Lanceur de compétences pour une entité de combat.
/// - Vérifie le tour via Phase_TurnByTurnCombat
/// - Cible une tuile au clic droit et applique la compétence équipée (Data_Skill)
/// - Gère portée, coût PA, zone d'impact et effets immédiats
/// - Notifie la Timeline pour rafraîchir les PV immédiatement après le cast
/// </summary>
[AddComponentMenu("Combat/Entity Skill Caster")]
public class Entity_SkillCaster : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   // [FR] Accès à phaseTurn + tileGrid
    public TileGrid_Manager tileGrid;          // [FR] Grille de combat

    [Header("Skill")]
    public Data_Skill equippedSkill;           // [FR] Sort sélectionné (UI)

    private Entity_StatistiqueCombat stats;

    // [FR] Compteur "lancers par cible" pour CE tour
    private readonly Dictionary<GameObject, int> perTargetCastCount = new();

    // =========================
    // Constructor / Destructor
    // =========================
    public Entity_SkillCaster() { }
    ~Entity_SkillCaster() { }

    private void Awake()
    {
        // [FR] Récupère les refs si non assignées dans l’inspector
        if (!phaseManager) phaseManager = FindAnyObjectByType<Combat_PhaseManager>(FindObjectsInactive.Include);
        if (!tileGrid && phaseManager) tileGrid = phaseManager.tileGrid;
    }

    private void Start()
    {
        // [FR] Cache la ref aux stats
        stats = GetComponent<Entity_StatistiqueCombat>();
    }

    private void Update()
    {
        if (!enabled || equippedSkill == null || stats == null) return;
        if (phaseManager == null || phaseManager.phaseTurn == null) return;
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        // [FR] Clic droit = tentative de lancer le sort
        if (Input.GetMouseButtonDown(1))
        {
            TryCastAtMouse();
        }
    }

    /// <summary>[FR] À appeler en début de tour pour réinitialiser les limites par cible.</summary>
    public void ResetSkillTurnUsage() => perTargetCastCount.Clear();

    // ---------------------------------------------------------------------
    private void TryCastAtMouse()
    {
        if (!Camera.main || tileGrid == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
        if (!hit.collider.TryGetComponent(out SetupTile tile)) return;

        TryCastAtTile(tile);
    }

    private void TryCastAtTile(SetupTile targetTile)
    {
        if (tileGrid == null) return;

        // [FR] Vérif PA
        if (stats.currentPA < equippedSkill.costPA)
        {
            Debug.Log("[Skill] PA insuffisants.");
            return;
        }

        // [FR] Vérif portée (Manhattan) + bonus PO courant
        Vector2Int from = GetCurrentCoord();
        Vector2Int to = new Vector2Int(targetTile.tileX, targetTile.tileY);
        int dist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        if (dist < equippedSkill.rangeMin || dist > equippedSkill.rangeMax + stats.currentPO)
        {
            Debug.Log("[Skill] Cible hors de portée.");
            return;
        }

        // [FR] Applique sur cibles
        if (equippedSkill.impactZone != null && equippedSkill.impactZone.zone != null &&
            equippedSkill.impactZone.zone.Length == 1)
        {
            // [FR] Monocible : entité sur la tuile cliquée
            var target = tileGrid.GetEntityOnTile(targetTile.gameObject);
            if (!target)
            {
                Debug.Log("[Skill] Aucune cible sur la tuile.");
                return;
            }

            if (!CheckPerTargetLimit(target)) return;

            ApplySkillOnTarget(target);
            IncrementPerTarget(target);
            stats.SetPA(stats.currentPA - equippedSkill.costPA);

            RequestTimelineHpRefresh(); // [FR] UI : met à jour la timeline immédiatement
        }
        else
        {
            // [FR] Zone : récupère toutes les cibles dans la zone d'impact
            var targets = GetTargetsInImpactZone(to);
            if (targets.Count == 0)
            {
                Debug.Log("[Skill] Aucune cible dans la zone.");
                return;
            }

            for (int i = 0; i < targets.Count; i++)
                ApplySkillOnTarget(targets[i]);

            stats.SetPA(stats.currentPA - equippedSkill.costPA);

            RequestTimelineHpRefresh(); // [FR] UI : met à jour la timeline immédiatement
        }
    }

    // ---------------------------------------------------------------------
    private Vector2Int GetCurrentCoord()
    {
        if (tileGrid == null) return Vector2Int.zero;

        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile s))
            return new Vector2Int(s.tileX, s.tileY);

        // [FR] Fallback : tuile la plus proche si dicos pas à jour
        Vector2Int best = Vector2Int.zero;
        float min = float.MaxValue;
        var tiles = tileGrid.GetAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            if (!t || !t.TryGetComponent(out SetupTile st)) continue;
            float d = Vector3.Distance(transform.position, t.transform.position);
            if (d < min) { min = d; best = new Vector2Int(st.tileX, st.tileY); }
        }
        return best;
    }

    private List<GameObject> GetTargetsInImpactZone(Vector2Int center)
    {
        var res = new List<GameObject>();
        if (tileGrid == null || equippedSkill == null ||
            equippedSkill.impactZone == null || equippedSkill.impactZone.zone == null)
            return res;

        var zone = equippedSkill.impactZone.zone;
        for (int i = 0; i < zone.Length; i++)
        {
            Vector2Int c = center + zone[i];
            var tile = tileGrid.GetTileAtCoordinates(c.x, c.y);
            if (!tile) continue;
            var occ = tileGrid.GetEntityOnTile(tile);
            if (occ) res.Add(occ);
        }
        return res;
    }

    private bool CheckPerTargetLimit(GameObject target)
    {
        if (equippedSkill.maxPerTargetPerTurn <= 0) return true;
        int count = perTargetCastCount.TryGetValue(target, out var v) ? v : 0;
        if (count >= equippedSkill.maxPerTargetPerTurn)
        {
            Debug.LogWarning($"[Skill] Limite par cible atteinte pour {equippedSkill.skillName} sur {target.name}.");
            return false;
        }
        return true;
    }

    private void IncrementPerTarget(GameObject target)
    {
        int count = perTargetCastCount.TryGetValue(target, out var v) ? v : 0;
        perTargetCastCount[target] = count + 1;
    }

    // ---------------------------------------------------------------------
    private void ApplySkillOnTarget(GameObject target)
    {
        if (!target || !target.TryGetComponent(out Entity_StatistiqueCombat ts)) return;

        // [FR] Critique : caster + skill
        float critChance = Mathf.Clamp(stats.currentCritChance + equippedSkill.critChance, 0f, 100f);
        bool isCrit = Random.value < (critChance / 100f);

        // [FR] Jet dégâts
        int jet = Random.Range(equippedSkill.damageMin, equippedSkill.damageMax + 1);

        // [FR] Multiplicateur selon stat élémentaire + résistance cible (en %)
        float attackerStat = GetOffensiveStat(stats, equippedSkill.skillElement);
        float statMult = (attackerStat + 100f) / 100f;
        float res = GetResistanceFor(ts, equippedSkill.skillElement);

        float dmg = jet * statMult;
        if (isCrit) dmg *= 1.5f;
        dmg *= (100f - res) / 100f;

        int final = Mathf.Max(0, Mathf.RoundToInt(dmg));

        // [FR] Pas de shield → applique directement aux PV
        if (final > 0) ts.SetHP(ts.currentHP - final);

        Debug.Log($"[Skill] {name} → {target.name} : {final} dmg{(isCrit ? " (CRIT)" : "")}");

        // [FR] Effets normaux (immédiats seulement)
        if (equippedSkill.effects != null && equippedSkill.effects.Count > 0)
        {
            for (int i = 0; i < equippedSkill.effects.Count; i++)
            {
                var eff = equippedSkill.effects[i];
                ApplyImmediateEffect(eff, eff.applyToSelf ? stats : ts);
            }
        }

        // [FR] Effets critiques (immédiats seulement)
        if (isCrit && equippedSkill.critEffects != null && equippedSkill.critEffects.Count > 0)
        {
            for (int i = 0; i < equippedSkill.critEffects.Count; i++)
            {
                var eff = equippedSkill.critEffects[i];
                ApplyImmediateEffect(eff, eff.applyToSelf ? stats : ts);
            }
        }
    }

    private void ApplyImmediateEffect(SkillEffect eff, Entity_StatistiqueCombat to)
    {
        if (eff == null || to == null) return;

        if (eff.duration > 0)
        {
            // [FR] On ne modifie pas ton runtime actuel : effet temporisé à gérer plus tard
            Debug.LogWarning($"[Skill] Timed effect TODO: {eff.effectType} ({eff.duration} tours).");
            return;
        }

        int v = Mathf.RoundToInt(eff.value);

        switch (eff.effectType)
        {
            // [FR] Vitalité / PA / PM / PO
            case EffectType.BonusPV: to.SetHP(to.currentHP + v); break;
            case EffectType.BonusPA: to.SetPA(to.currentPA + v); break;
            case EffectType.MalusPA: to.SetPA(to.currentPA - v); break;
            case EffectType.BonusPM: to.SetPM(to.currentPM + v); break;
            case EffectType.MalusPM: to.SetPM(to.currentPM - v); break;
            case EffectType.BonusPO: to.SetPO(to.currentPO + v); break;
            case EffectType.MalusPO: to.SetPO(to.currentPO - v); break;

            // [FR] Caractéristiques
            case EffectType.BonusFor: to.SetForce(to.currentForce + v); break;
            case EffectType.MalusFor: to.SetForce(to.currentForce - v); break;
            case EffectType.BonusDex: to.SetDex(to.currentDexterite + v); break;
            case EffectType.MalusDex: to.SetDex(to.currentDexterite - v); break;
            case EffectType.BonusMag: to.SetMagie(to.currentMagie + v); break;
            case EffectType.MalusMag: to.SetMagie(to.currentMagie - v); break;
            case EffectType.BonusFoi: to.SetFoi(to.currentFoi + v); break;
            case EffectType.MalusFoi: to.SetFoi(to.currentFoi - v); break;

            // [FR] Résistances
            case EffectType.BonusResFor: to.SetResForce(to.currentResistanceForce + v); break;
            case EffectType.MalusResFor: to.SetResForce(to.currentResistanceForce - v); break;
            case EffectType.BonusResDex: to.SetResDex(to.currentResistanceDexterite + v); break;
            case EffectType.MalusResDex: to.SetResDex(to.currentResistanceDexterite - v); break;
            case EffectType.BonusResMag: to.SetResMagie(to.currentResistanceMagie + v); break;
            case EffectType.MalusResMag: to.SetResMagie(to.currentResistanceMagie - v); break;
            case EffectType.BonusResFoi: to.SetResFoi(to.currentResistanceFoi + v); break;
            case EffectType.MalusResFoi: to.SetResFoi(to.currentResistanceFoi - v); break;

            default:
                Debug.Log($"[Skill] Immediate effect not handled: {eff.effectType}");
                break;
        }
    }

    // ---------------------------------------------------------------------
    // Helpers internes

    private float GetOffensiveStat(Entity_StatistiqueCombat s, SkillElement element)
    {
        switch (element)
        {
            case SkillElement.Force: return s.currentForce;
            case SkillElement.Dexterité: return s.currentDexterite;   // [FR] enum avec accent
            case SkillElement.Magie: return s.currentMagie;
            case SkillElement.Foi: return s.currentFoi;
            default: return s.currentForce;
        }
    }

    private float GetResistanceFor(Entity_StatistiqueCombat s, SkillElement element)
    {
        switch (element)
        {
            case SkillElement.Force: return s.currentResistanceForce;
            case SkillElement.Dexterité: return s.currentResistanceDexterite; // [FR] enum avec accent
            case SkillElement.Magie: return s.currentResistanceMagie;
            case SkillElement.Foi: return s.currentResistanceFoi;
            default: return 0f;
        }
    }

    private void RequestTimelineHpRefresh()
    {
        // [FR] Notifie la phase de rafraîchir l'UI des PV (Timeline)
        if (phaseManager != null && phaseManager.phaseTurn != null)
            phaseManager.phaseTurn.RefreshTimelineHP();
    }
}
