using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///   Lanceur de compétences pour une entité de combat.
/// - Gère l'équipement et le lancement d'un Data_Skill (clic souris OU API publique pour l'IA)
/// - Vérifie PA, portée (Manhattan + PO), zone d'impact, limites par cible, application des effets
/// - Déclenche un FX 2D (via Skill_Binding) sur la case ciblée, avec offset Y
/// - Peut optionnellement attendre la fin de l'animation avant d'appliquer les effets
/// - Met la case ciblée en rouge pendant le FX + calcul
/// - Notifie la Timeline pour rafraîchir les PV immédiatement après un cast
/// </summary>
public class Entity_SkillCaster : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   //   Assigné/injecté (aucun auto-find)
    public TileGrid_Manager tileGrid;          //   Assigné/injecté (aucun auto-find)

    [Header("Skill")]
    public Data_Skill equippedSkill;           //   Sort sélectionné (UI joueur ou IA via EquipSkill)

    [Header("FX")]
    public bool waitFxBeforeApply = false;     //   Si vrai: on attend la fin du FX avant d'appliquer les effets
    public Transform fxParent;                 //   Parent optionnel pour les FX (ex: un "FXRoot" dans la scène)

    private Entity_StatistiqueCombat stats;

    //   Limite "(skill, cible) par tour"
    private readonly Dictionary<(Data_Skill, GameObject), int> perTargetPerSkillThisTurn = new();

    // =========================
    // Constructor / Destructor
    // =========================
    public Entity_SkillCaster() { }
    ~Entity_SkillCaster() { }

    private void Start()
    {
        stats = GetComponent<Entity_StatistiqueCombat>();
    }

    private void Update()
    {
        if (!enabled || stats == null) return;
        if (phaseManager == null || phaseManager.phaseTurn == null) return;
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        //   Ne pas caster si la souris est sur une UI (boutons, etc.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        //   CLIC DROIT = lancer le sort sur la tuile sous la souris
        if (equippedSkill != null && Input.GetMouseButtonDown(1))
            TryCastAtMouse();
    }

    // =====================================================================
    // ========================  INTERACTION SOURIS  =======================
    // =====================================================================

    private void TryCastAtMouse()
    {
        if (!Camera.main || tileGrid == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
        if (!hit.collider.TryGetComponent(out SetupTile tile)) return;

        GameObject tileGO = tile.gameObject;

        //   Au choix: on attend le FX, ou on applique tout de suite après avoir déclenché le FX
        if (waitFxBeforeApply)
        {
            StartCoroutine(CastAtTile_PlayFxThenApply_WithHighlight(equippedSkill, tileGO));
        }
        else
        {
            //   Déclenche le FX immédiatement (fire-and-forget) + highlight pendant l'anim
            var setup = tile.GetComponent<SetupTile>();
            Tile_State prev = HighlightTile(setup, true); // rouge

            var fxInst = PlayFxFor_GetInstance(equippedSkill, tileGO); // peut être null si pas de FX
            CastAtTile(equippedSkill, tileGO);                         // calculs + dégâts maintenant

            //   On enlève le rouge quand le FX se termine (ou très vite si pas de FX)
            StartCoroutine(UnhighlightAfterRunner(setup, prev, fxInst));
        }
    }

    // =====================================================================
    // =====================     API PUBLIQUE POUR IA     ==================
    // =====================================================================

    /// <summary>
    ///   Équipe un skill (utilisé par l'UI joueur ou l'IA).
    /// </summary>
    public void EquipSkill(Data_Skill skill)
    {
        equippedSkill = skill;
    }

    /// <summary>
    ///   API IA: lance un skill sur une tuile, avec FX + highlight selon waitFxBeforeApply.
    ///      À utiliser avec: yield return caster.CastAtTileWithFx(skill, tile);
    /// </summary>
    public IEnumerator CastAtTileWithFx(Data_Skill skill, GameObject targetTile)
    {
        if (targetTile == null || !targetTile.TryGetComponent(out SetupTile setup)) yield break;

        if (waitFxBeforeApply)
        {
            yield return CastAtTile_PlayFxThenApply_WithHighlight(skill, targetTile);
        }
        else
        {
            Tile_State prev = HighlightTile(setup, true);
            var fxInst = PlayFxFor_GetInstance(skill, targetTile);
            CastAtTile(skill, targetTile);
            yield return UnhighlightAfterRunner(setup, prev, fxInst);
        }
    }

    /// <summary>
    ///   Vérifie si 'skill' peut être lancé sur la tuile 'targetTile' (sans consommer).
    ///   Autorise désormais le cast sur case VIDE (mono et zone).
    /// </summary>
    public bool CanCastAtTile(Data_Skill skill, GameObject targetTile, out string reason)
    {
        reason = "";
        if (skill == null) { reason = "Skill null."; return false; }
        if (tileGrid == null) { reason = "TileGrid null."; return false; }
        if (stats == null) { reason = "Stats null."; return false; }
        if (targetTile == null || !targetTile.TryGetComponent(out SetupTile setup)) { reason = "Tuile invalide."; return false; }

        //   PA suffisants ?
        if (stats.currentPA < skill.costPA) { reason = "PA insuffisants."; return false; }

        //   Portée (Manhattan) + bonus de PO
        Vector2Int from = GetCurrentCoord();
        Vector2Int to = new Vector2Int(setup.tileX, setup.tileY);
        int dist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

        if (dist < skill.rangeMin) { reason = "Trop proche."; return false; }
        if (dist > skill.rangeMax + stats.currentPO) { reason = "Trop loin."; return false; }

        //   MONO-CIBLE :
        // - On n'exige plus la présence d'une entité.
        // - Si une entité est là, on applique la limite par cible, sinon on laisse passer.
        if (IsSingleTarget(skill))
        {
            var maybe = tileGrid.GetEntityOnTile(targetTile);
            if (maybe != null)
            {
                if (!CheckPerTargetLimit(skill, maybe)) { reason = "Limite par cible atteinte."; return false; }
            }
            return true;
        }

        //   ZONE : plus de blocage si aucune entité n'est touchée (cast autorisé).
        return true;
    }

    /// <summary>
    ///   Lance effectivement 'skill' sur la tuile 'targetTile' (si autorisé).
    ///   Ne gère PAS le FX ici (le FX est déclenché ailleurs).
    /// </summary>
    public bool CastAtTile(Data_Skill skill, GameObject targetTile)
    {
        if (!CanCastAtTile(skill, targetTile, out string why))
        {
            Debug.LogWarning("[Caster] Cast refusé: " + why);
            return false;
        }

        if (!targetTile.TryGetComponent(out SetupTile setup)) return false;

        bool isMono = IsSingleTarget(skill);

        //   Swap temporaire pour réutiliser les helpers basés sur 'equippedSkill'
        var saved = equippedSkill;
        equippedSkill = skill;

        if (isMono)
        {
            //   Case vide désormais acceptée : si pas d'entité, on consomme juste les PA (pas d'effet).
            var target = tileGrid.GetEntityOnTile(targetTile);
            if (target)
            {
                if (!CheckPerTargetLimit(skill, target)) { equippedSkill = saved; return false; }
                ApplySkillOnTarget(skill, target);
                IncrementPerTarget(skill, target);
            }
            stats.SetPA(stats.currentPA - skill.costPA);
        }
        else
        {
            //   Zone : même si aucun ennemi touché, on cast et on consomme les PA.
            Vector2Int center = new Vector2Int(setup.tileX, setup.tileY);
            var targets = GetTargetsInImpactZone(skill, center); // peut être vide
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                if (!CheckPerTargetLimit(skill, t)) continue;
                ApplySkillOnTarget(skill, t);
                IncrementPerTarget(skill, t);
            }
            stats.SetPA(stats.currentPA - skill.costPA);
        }

        equippedSkill = saved;

        //   UI: rafraîchit Timeline immédiatement
        RequestTimelineHpRefresh();
        return true;
    }

    /// <summary>
    ///   Coroutine: met la tuile en rouge, joue le FX, attend la fin, applique le skill, restaure la tuile.
    /// </summary>
    private IEnumerator CastAtTile_PlayFxThenApply_WithHighlight(Data_Skill skill, GameObject targetTile)
    {
        //   Vérif rapide (hors FX) pour ne pas jouer un FX si le cast est impossible
        if (!CanCastAtTile(skill, targetTile, out _)) yield break;
        if (!targetTile.TryGetComponent(out SetupTile setup)) yield break;

        //   1) Tuile ciblée en rouge
        Tile_State prev = HighlightTile(setup, true);

        //   2) FX et attente de fin
        yield return PlayFxAndWaitFor(skill, targetTile);

        //   3) Appliquer le skill (logique standard)
        CastAtTile(skill, targetTile);

        //   4) Restaure la tuile
        HighlightTile(setup, false, prev);
    }

    /// <summary>
    ///   À appeler en début de tour pour réinitialiser les limites par cible & skill.
    /// </summary>
    public void ResetSkillTurnUsage()
    {
        perTargetPerSkillThisTurn.Clear();
    }

    // =====================================================================
    // ============================  HELPERS  ===============================
    // =====================================================================

    //   Déclenche le FX et retourne l'instance (ou null) pour pouvoir attendre sa fin.
    private Sprite_AnimationRunner PlayFxFor_GetInstance(Data_Skill skill, GameObject targetTile)
    {
        var binding = FindBindingForSkill(skill);
        if (binding == null) return null;

        Vector3 basePos = targetTile.transform.position;
        return Skill_FXHelper.PlayFx(binding, basePos, fxParent);
    }

    //   Joue le FX et attend la fin (si présent).
    private IEnumerator PlayFxAndWaitFor(Data_Skill skill, GameObject targetTile)
    {
        var binding = FindBindingForSkill(skill);
        if (binding == null) yield break;

        Vector3 basePos = targetTile.transform.position;
        yield return Skill_FXHelper.PlayFxAndWait(binding, basePos, fxParent);
    }

    //   Retrouve, dans le SkillBook de l'entité, le binding (Skill + FX) pour ce skill.
    private Skill_Binding FindBindingForSkill(Data_Skill s)
    {
        if (stats == null || stats.skillBook == null || s == null) return null;
        for (int i = 0; i < stats.skillBook.Count; i++)
        {
            var b = stats.skillBook[i];
            if (b != null && b.skill == s) return b;
        }
        return null;
    }

    private static bool IsSingleTarget(Data_Skill skill)
    {
        //   Mono-cible si:
        //  - impactZone == null, ou zone == null, ou zone.Length == 0
        //  - OU zone == [ (0,0) ]
        if (skill == null || skill.impactZone == null || skill.impactZone.zone == null) return true;
        var z = skill.impactZone.zone;
        if (z.Length == 0) return true;
        if (z.Length == 1 && z[0] == Vector2Int.zero) return true;
        return z.Length == 1; //   par sécurité : une seule case relative
    }

    private Vector2Int GetCurrentCoord()
    {
        if (tileGrid == null) return Vector2Int.zero;

        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile s))
            return new Vector2Int(s.tileX, s.tileY);

        //   Fallback : tuile la plus proche si dicos pas à jour
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

    private List<GameObject> GetTargetsInImpactZone(Data_Skill skill, Vector2Int center)
    {
        var res = new List<GameObject>();
        if (tileGrid == null || skill == null || skill.impactZone == null || skill.impactZone.zone == null)
            return res;

        var zone = skill.impactZone.zone;
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

    private bool CheckPerTargetLimit(Data_Skill skill, GameObject target)
    {
        if (skill == null || target == null) return false;

        if (skill.maxPerTargetPerTurn <= 0) return true;

        var key = (skill, target);
        int count = perTargetPerSkillThisTurn.TryGetValue(key, out var v) ? v : 0;
        if (count >= skill.maxPerTargetPerTurn)
        {
            Debug.LogWarning($"[Skill] Limite par cible atteinte pour {skill.skillName} sur {target.name}.");
            return false;
        }
        return true;
    }

    private void IncrementPerTarget(Data_Skill skill, GameObject target)
    {
        var key = (skill, target);
        int count = perTargetPerSkillThisTurn.TryGetValue(key, out var v) ? v : 0;
        perTargetPerSkillThisTurn[key] = count + 1;
    }

    private void ApplySkillOnTarget(Data_Skill skill, GameObject target)
    {
        if (skill == null || !target || !target.TryGetComponent(out Entity_StatistiqueCombat ts)) return;

        //   IMPORTANT : on ne s'inflige jamais de dégâts à soi-même
        if (ts == stats)
        {
            //   Les effets "applyToSelf" seront gérés plus bas. On saute le calcul de dégâts sur soi.
            // Debug.Log("[Skill] Auto-dégâts ignorés pour le lanceur.");
        }
        else
        {
            //   Critique : caster + skill
            float critChance = Mathf.Clamp(stats.currentCritChance + skill.critChance, 0f, 100f);
            bool isCrit = Random.value < (critChance / 100f);

            //   Jet dégâts
            int jet = Random.Range(skill.damageMin, skill.damageMax + 1);

            //   Multiplicateur selon stat élémentaire + résistance cible (en %)
            float attackerStat = GetOffensiveStat(stats, skill.skillElement);
            float statMult = (attackerStat + 100f) / 100f;
            float res = GetResistanceFor(ts, skill.skillElement);

            float dmg = jet * statMult;
            if (isCrit) dmg *= 1.5f;
            dmg *= (100f - res) / 100f;

            int final = Mathf.Max(0, Mathf.RoundToInt(dmg));

            //   Pas de shield → applique directement aux PV
            if (final > 0) ts.SetHP(ts.currentHP - final);

            Debug.Log($"[Skill] {name} → {target.name} : {final} dmg{(isCrit ? " (CRIT)" : "")}");
        }

        //   Effets non-crit (les "applyToSelf" utilisent 'stats' comme receiver)
        if (skill.effects != null && skill.effects.Count > 0)
        {
            for (int i = 0; i < skill.effects.Count; i++)
            {
                var eff = skill.effects[i];
                var receiver = eff.applyToSelf ? stats : ts;

                if (eff.duration > 0)
                {
                    if (receiver.activeEffects == null) receiver.activeEffects = new List<Entity_StatistiqueCombat.ActiveEffect>();
                    receiver.activeEffects.Add(new Entity_StatistiqueCombat.ActiveEffect(eff));
                }
                else
                {
                    ApplyImmediateEffect(eff, receiver);
                }
            }
        }

        //   Effets critiques (si critique)
        float critChanceCheck = Mathf.Clamp(stats.currentCritChance + skill.critChance, 0f, 100f);
        bool wasCrit = Random.value < (critChanceCheck / 100f); //   simple check indépendant (selon ton design tu peux partager le roll)
        if (wasCrit && skill.critEffects != null && skill.critEffects.Count > 0)
        {
            for (int i = 0; i < skill.critEffects.Count; i++)
            {
                var eff = skill.critEffects[i];
                var receiver = eff.applyToSelf ? stats : ts;

                if (eff.duration > 0)
                {
                    if (receiver.activeEffects == null) receiver.activeEffects = new List<Entity_StatistiqueCombat.ActiveEffect>();
                    receiver.activeEffects.Add(new Entity_StatistiqueCombat.ActiveEffect(eff));
                }
                else
                {
                    ApplyImmediateEffect(eff, receiver);
                }
            }
        }
    }

    private void ApplyImmediateEffect(SkillEffect eff, Entity_StatistiqueCombat to)
    {
        if (eff == null || to == null) return;

        int v = Mathf.RoundToInt(eff.value);

        switch (eff.effectType)
        {
            //   Vitalité / PA / PM / PO (instantanés)
            case EffectType.BonusPV: to.SetHP(to.currentHP + v); break;
            case EffectType.BonusPA: to.SetPA(to.currentPA + v); break;
            case EffectType.MalusPA: to.SetPA(to.currentPA - v); break;
            case EffectType.BonusPM: to.SetPM(to.currentPM + v); break;
            case EffectType.MalusPM: to.SetPM(to.currentPM - v); break;
            case EffectType.BonusPO: to.SetPO(to.currentPO + v); break;
            case EffectType.MalusPO: to.SetPO(to.currentPO - v); break;

            //   Caractéristiques
            case EffectType.BonusFor: to.SetForce(to.currentForce + v); break;
            case EffectType.MalusFor: to.SetForce(to.currentForce - v); break;
            case EffectType.BonusDex: to.SetDex(to.currentDexterite + v); break;
            case EffectType.MalusDex: to.SetDex(to.currentDexterite - v); break;
            case EffectType.BonusMag: to.SetMagie(to.currentMagie + v); break;
            case EffectType.MalusMag: to.SetMagie(to.currentMagie - v); break;
            case EffectType.BonusFoi: to.SetFoi(to.currentFoi + v); break;
            case EffectType.MalusFoi: to.SetFoi(to.currentFoi - v); break;

            //   Résistances
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

    //   Met/retire l'état rouge sur la tuile visée. Retourne l'état précédent.
    private Tile_State HighlightTile(SetupTile setup, bool on, Tile_State restoreTo = Tile_State.None)
    {
        if (setup == null) return Tile_State.None;

        Tile_State prev = setup.currentState;
        if (on) setup.currentState = Tile_State.TeamRed;
        else setup.currentState = restoreTo;

        return prev;
    }

    private IEnumerator UnhighlightAfterRunner(SetupTile setup, Tile_State prev, Sprite_AnimationRunner inst)
    {
        if (inst != null)
            yield return inst.WaitForCompletion();
        else
            yield return null; //   pas de FX → état rétabli au tick suivant

        HighlightTile(setup, false, prev);
    }

    private float GetOffensiveStat(Entity_StatistiqueCombat s, SkillElement element)
    {
        switch (element)
        {
            case SkillElement.Force: return s.currentForce;
            case SkillElement.Dexterité: return s.currentDexterite;
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
            case SkillElement.Dexterité: return s.currentResistanceDexterite;
            case SkillElement.Magie: return s.currentResistanceMagie;
            case SkillElement.Foi: return s.currentResistanceFoi;
            default: return 0f;
        }
    }

    private void RequestTimelineHpRefresh()
    {
        if (phaseManager != null && phaseManager.phaseTurn != null)
            phaseManager.phaseTurn.RefreshTimelineHP();
    }
}
