using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Entity_SkillCaster : MonoBehaviour
{
    [Header("References")]
    public Combat_PhaseManager phaseManager;   // Assigné/injecté
    public TileGrid_Manager tileGrid;          // Assigné/injecté
    public Entity_Animations anim;             // Référence vers le contrôleur d’animations 3D

    [Header("Skill")]
    public Data_Skill equippedSkill;           // Sort sélectionné (UI/IA)

    [Header("FX")]
    public bool waitFxBeforeApply = false;     // Si vrai: attendre la fin du FX avant les effets
    public Transform fxParent;                 // Parent optionnel pour les FX

    [Header("Control")]
    public bool acceptHumanInput = true;       // False pour IA

    [Header("Damage Tuning")]
    [SerializeField] private float critMultiplier = 1.5f;   // Multiplicateur critique
    [SerializeField] private bool clampResist = true;        // Clamp des résistances
    [SerializeField] private Vector2 resistClamp = new Vector2(-100f, 100f); // Min/Max en %

    [Header("Orientation")]
    public bool rotateTowardsTarget = true;    // Si true, on oriente l’entité vers la tuile ciblée lors d’un cast
    public bool instantTurnOnCast = true;      // Si true, pivot instantané au lancement du sort
    public float rotateSpeedDeg = 540f;        // Vitesse de rotation si pivot non instantané (degrés/s)
    public float rotationOffsetY = 0f;         // Offset pour corriger un prefab mal orienté (ex: 90, -90, 180)

    private Entity_StatistiqueCombat stats;
    private Popup_DisplayNumber popup;         // Pop-up PA du lanceur

    // Limite "(skill, cible) par tour"
    private readonly Dictionary<(Data_Skill, GameObject), int> perTargetPerSkillThisTurn = new();

    private void Start()
    {
        // Récupérations locales
        stats = GetComponent<Entity_StatistiqueCombat>();
        popup = GetComponent<Popup_DisplayNumber>();

        // Récupération de sécurité si non assigné dans l’Inspector
        if (anim == null) TryGetComponent(out anim);
    }

    private void Update()
    {
        if (!enabled || stats == null) return;
        if (phaseManager == null || phaseManager.phaseTurn == null) return;

        // Uniquement au tour de CETTE entité
        if (!phaseManager.phaseTurn.IsMyTurn(gameObject)) return;

        // Ignore la souris si IA
        if (!acceptHumanInput) return;

        // Pas de cast si on pointe une UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Clic droit → tenter le cast sur la tuile sous la souris
        if (equippedSkill != null && Input.GetMouseButtonDown(1))
            TryCastAtMouse();
    }

    // ========================  INTERACTION SOURIS  =======================

    private void TryCastAtMouse()
    {
        if (!Camera.main || tileGrid == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;
        if (!hit.collider.TryGetComponent(out SetupTile tile)) return;

        GameObject tileGO = tile.gameObject;

        // Oriente l’entité vers la tuile ciblée juste avant de jouer l’animation
        OrientTowardsTile(tileGO);

        if (waitFxBeforeApply)
        {
            // Avec attente: l’animation sera déclenchée dans la coroutine
            StartCoroutine(CastAtTile_PlayFxThenApply_WithHighlight(equippedSkill, tileGO));
        }
        else
        {
            // Sans attente: on joue l’animation d’attaque avant FX/effets
            PlayAttackAnimationForSkill(equippedSkill);

            var setup = tile.GetComponent<SetupTile>();
            Tile_State prev = HighlightTile(setup, true);         // rouge visuel

            var fxInst = PlayFxFor_GetInstance(equippedSkill, tileGO);
            CastAtTile(equippedSkill, tileGO);                    // calculs + dégâts

            StartCoroutine(UnhighlightAfterRunner(setup, prev, fxInst));
        }
    }

    // =====================     API PUBLIQUE POUR IA     ==================

    public void EquipSkill(Data_Skill skill)
    {
        equippedSkill = skill;
    }

    public IEnumerator CastAtTileWithFx(Data_Skill skill, GameObject targetTile)
    {
        if (targetTile == null || !targetTile.TryGetComponent(out SetupTile setup)) yield break;

        // Oriente l’entité vers la tuile ciblée avant l’animation
        OrientTowardsTile(targetTile);

        if (waitFxBeforeApply)
        {
            // Avec attente: l’animation est déclenchée dans la coroutine appelée
            yield return CastAtTile_PlayFxThenApply_WithHighlight(skill, targetTile);
        }
        else
        {
            // Sans attente: jouer l’animation d’attaque maintenant
            PlayAttackAnimationForSkill(skill);

            Tile_State prev = HighlightTile(setup, true);
            var fxInst = PlayFxFor_GetInstance(skill, targetTile);
            CastAtTile(skill, targetTile);
            yield return UnhighlightAfterRunner(setup, prev, fxInst);
        }
    }

    public bool CanCastAtTile(Data_Skill skill, GameObject targetTile, out string reason)
    {
        reason = "";
        if (skill == null) { reason = "Skill null."; return false; }
        if (tileGrid == null) { reason = "TileGrid null."; return false; }
        if (stats == null) { reason = "Stats null."; return false; }
        if (targetTile == null || !targetTile.TryGetComponent(out SetupTile setup)) { reason = "Tuile invalide."; return false; }

        if (stats.currentPA < skill.costPA) { reason = "PA insuffisants."; return false; }

        Vector2Int from = GetCurrentCoord();
        Vector2Int to = new Vector2Int(setup.tileX, setup.tileY);
        int dist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

        if (dist < skill.rangeMin) { reason = "Trop proche."; return false; }
        if (dist > skill.rangeMax + stats.currentPO) { reason = "Trop loin."; return false; }

        if (IsSingleTarget(skill))
        {
            var maybe = tileGrid.GetEntityOnTile(targetTile);
            if (maybe != null)
            {
                if (!CheckPerTargetLimit(skill, maybe)) { reason = "Limite par cible atteinte."; return false; }
            }
            return true;
        }

        return true;
    }

    public bool CastAtTile(Data_Skill skill, GameObject targetTile)
    {
        if (!CanCastAtTile(skill, targetTile, out string why))
        {
            Debug.LogWarning("[Caster] Cast refusé: " + why);
            return false;
        }

        if (!targetTile.TryGetComponent(out SetupTile setup)) return false;
        bool isMono = IsSingleTarget(skill);

        // Swap temporaire pour réutiliser les helpers basés sur 'equippedSkill'
        var saved = equippedSkill;
        equippedSkill = skill;

        if (isMono)
        {
            // Case vide autorisée
            var target = tileGrid.GetEntityOnTile(targetTile);
            if (target)
            {
                if (!CheckPerTargetLimit(skill, target)) { equippedSkill = saved; return false; }
                ApplySkillOnTarget(skill, target);
                IncrementPerTarget(skill, target);
            }

            // Consomme PA + pop-up du lanceur
            stats.SetPA(stats.currentPA - skill.costPA);
            if (popup != null) popup.ShowPA(skill.costPA);
        }
        else
        {
            // Zone
            Vector2Int center = new Vector2Int(setup.tileX, setup.tileY);
            var targets = GetTargetsInImpactZone(skill, center);
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                if (!CheckPerTargetLimit(skill, t)) continue;
                ApplySkillOnTarget(skill, t);
                IncrementPerTarget(skill, t);
            }

            // Consomme PA + pop-up du lanceur
            stats.SetPA(stats.currentPA - skill.costPA);
            if (popup != null) popup.ShowPA(skill.costPA);
        }

        equippedSkill = saved;

        // UI: rafraîchit Timeline immédiatement
        RequestTimelineHpRefresh();
        return true;
    }

    private IEnumerator CastAtTile_PlayFxThenApply_WithHighlight(Data_Skill skill, GameObject targetTile)
    {
        if (!CanCastAtTile(skill, targetTile, out _)) yield break;
        if (!targetTile.TryGetComponent(out SetupTile setup)) yield break;

        // Oriente l’entité vers la tuile ciblée avant de jouer l’animation
        OrientTowardsTile(targetTile);

        // Avec attente: déclencher l’animation d’attaque AVANT les FX
        PlayAttackAnimationForSkill(skill);

        Tile_State prev = HighlightTile(setup, true);
        yield return PlayFxAndWaitFor(skill, targetTile);
        CastAtTile(skill, targetTile);
        HighlightTile(setup, false, prev);
    }

    public void ResetSkillTurnUsage()
    {
        perTargetPerSkillThisTurn.Clear();
    }

    // ============================  HELPERS  ===============================

    private Sprite_AnimationRunner PlayFxFor_GetInstance(Data_Skill skill, GameObject targetTile)
    {
        var binding = FindBindingForSkill(skill);
        if (binding == null) return null;

        Vector3 basePos = targetTile.transform.position;
        return Skill_FXHelper.PlayFx(binding, basePos, fxParent);
    }

    private IEnumerator PlayFxAndWaitFor(Data_Skill skill, GameObject targetTile)
    {
        var binding = FindBindingForSkill(skill);
        if (binding == null) yield break;

        Vector3 basePos = targetTile.transform.position;
        yield return Skill_FXHelper.PlayFxAndWait(binding, basePos, fxParent);
    }

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
        if (skill == null || skill.impactZone == null || skill.impactZone.zone == null) return true;
        var z = skill.impactZone.zone;
        if (z.Length == 0) return true;
        if (z.Length == 1 && z[0] == Vector2Int.zero) return true;
        return z.Length == 1;
    }

    private Vector2Int GetCurrentCoord()
    {
        if (tileGrid == null) return Vector2Int.zero;

        var tileObj = tileGrid.GetTileOfEntity(gameObject);
        if (tileObj && tileObj.TryGetComponent(out SetupTile s))
            return new Vector2Int(s.tileX, s.tileY);

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

        // On ne s'inflige jamais de dégâts à soi-même
        if (ts != stats)
        {
            // Calcul complet des dégâts
            bool isCrit;
            int final = ComputeFinalDamage(skill, stats, ts, out isCrit);

            // Application directe (pas de bouclier ici)
            if (final > 0)
            {
                ts.SetHP(ts.currentHP - final);

                var targetPopup = target.GetComponent<Popup_DisplayNumber>();
                if (targetPopup != null) targetPopup.ShowDamage(final);
            }

            Debug.Log($"[Skill] {name} → {target.name} : {final} dmg{(isCrit ? " (CRIT)" : "")}");
        }

        // Effets non-critiques
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

        // Effets critiques
        float critChanceCheck = Mathf.Clamp(stats.currentCritChance + skill.critChance, 0f, 100f);
        bool wasCrit = Random.value < (critChanceCheck / 100f);
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

    // =======================  CALCUL DES DÉGÂTS  =========================

    private int ComputeFinalDamage(Data_Skill skill, Entity_StatistiqueCombat attacker, Entity_StatistiqueCombat defender, out bool isCrit)
    {
        isCrit = false;
        if (skill == null || attacker == null || defender == null) return 0;

        int roll = Random.Range(skill.damageMin, skill.damageMax + 1);

        float offStat = GetOffensiveStat(attacker, skill.skillElement);
        float multStat = (100f + Mathf.Max(0f, offStat)) / 100f;

        float res = GetResistanceFor(defender, skill.skillElement);
        if (clampResist) res = Mathf.Clamp(res, resistClamp.x, resistClamp.y);
        float multRes = (100f - res) / 100f;

        float critChance = Mathf.Clamp(attacker.currentCritChance + skill.critChance, 0f, 100f);
        if (Random.value < (critChance / 100f))
        {
            isCrit = true;
        }

        float dmg = roll * multStat * multRes;
        if (isCrit) dmg *= Mathf.Max(1f, critMultiplier);

        int final = Mathf.Max(0, Mathf.RoundToInt(dmg));
        return final;
    }

    // ========================  EFFETS IMMÉDIATS  =========================

    private void ApplyImmediateEffect(SkillEffect eff, Entity_StatistiqueCombat to)
    {
        if (eff == null || to == null) return;

        int v = Mathf.RoundToInt(eff.value);

        switch (eff.effectType)
        {
            // Vitalité / PA / PM / PO
            case EffectType.BonusPV: to.SetHP(to.currentHP + v); break;
            case EffectType.BonusPA: to.SetPA(to.currentPA + v); break;
            case EffectType.MalusPA: to.SetPA(to.currentPA - v); break;
            case EffectType.BonusPM: to.SetPM(to.currentPM + v); break;
            case EffectType.MalusPM: to.SetPM(to.currentPM - v); break;
            case EffectType.BonusPO: to.SetPO(to.currentPO + v); break;
            case EffectType.MalusPO: to.SetPO(to.currentPO - v); break;

            // Caractéristiques
            case EffectType.BonusFor: to.SetForce(to.currentForce + v); break;
            case EffectType.MalusFor: to.SetForce(to.currentForce - v); break;
            case EffectType.BonusDex: to.SetDex(to.currentDexterite + v); break;
            case EffectType.MalusDex: to.SetDex(to.currentDexterite - v); break;
            case EffectType.BonusMag: to.SetMagie(to.currentMagie + v); break;
            case EffectType.MalusMag: to.SetMagie(to.currentMagie - v); break;
            case EffectType.BonusFoi: to.SetFoi(to.currentFoi + v); break;
            case EffectType.MalusFoi: to.SetFoi(to.currentFoi - v); break;

            // Résistances
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

    // =============================  ORIENTATION  =========================

    // Oriente l'entité vers la tuile ciblée (Y verrouillé), selon la configuration instant/progressif
    private void OrientTowardsTile(GameObject tileGO)
    {
        if (!rotateTowardsTarget || tileGO == null) return;

        Vector3 target = tileGO.transform.position;
        target.y = transform.position.y; // on reste à la même altitude

        RotateTowards(target, instantTurnOnCast);
    }

    // Oriente vers une position monde cible (plan XZ), offset appliqué pour corriger l'orientation du prefab
    private void RotateTowards(Vector3 worldTarget, bool instant)
    {
        if (!rotateTowardsTarget) return;

        Vector3 dir = worldTarget - transform.position;
        dir.y = 0f; // ignore toute pente verticale
        if (dir.sqrMagnitude < 0.000001f) return;

        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        yaw += rotationOffsetY;

        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);

        if (instant || rotateSpeedDeg <= 0f)
        {
            // Pivot immédiat
            transform.rotation = targetRot;
        }
        else
        {
            // Rotation progressive (appelée une fois ici, utile si la vitesse est élevée)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeedDeg * Time.deltaTime
            );
        }
    }

    // =============================  UTILS  ===============================

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
        if (inst != null) yield return inst.WaitForCompletion();
        else yield return null;
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

    // =========================  ANIMATIONS 3D  ===========================

    // Déclenche l’animation d’attaque en fonction de l’élément du sort
    private void PlayAttackAnimationForSkill(Data_Skill skill)
    {
        // Vérifie que l’anim est disponible
        if (anim == null || !anim.isActiveAndEnabled || skill == null) return;

        // Force/Dexterité => Physique ; Magie/Foi => Magique
        switch (skill.skillElement)
        {
            case SkillElement.Force:
            case SkillElement.Dexterité:
                anim.PlayCastPhysic();
                break;

            case SkillElement.Magie:
            case SkillElement.Foi:
                anim.PlayCastMagic();
                break;

            default:
                anim.PlayCastPhysic(); // Sécurité par défaut
                break;
        }
    }
}
