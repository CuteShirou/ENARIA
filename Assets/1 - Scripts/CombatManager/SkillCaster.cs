using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class SkillCaster : NetworkBehaviour
{
    public SkillData equippedSkill;
    public Grid gridManager;

    private CombatStats stats;
    private Camera mainCamera;

    private Dictionary<GameObject, int> perTargetCastCount = new Dictionary<GameObject, int>();

    private void Start()
    {
        stats = GetComponent<CombatStats>();
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (equippedSkill == null || stats == null || stats.currentPA < equippedSkill.costPA)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                TileCoord tile = hit.collider.GetComponent<TileCoord>();
                if (tile == null) return;

                CmdRequestCastSkill(tile.Coord);
            }
        }
    }

    [Command]
    private void CmdRequestCastSkill(Vector2Int targetCoord)
    {
        if (equippedSkill == null || stats == null || stats.currentPA < equippedSkill.costPA)
            return;

        if (!gridManager.TileMap.ContainsKey(targetCoord))
            return;

        Vector2Int from = GetCurrentCoord();
        int distance = Mathf.Abs(from.x - targetCoord.x) + Mathf.Abs(from.y - targetCoord.y);

        if (distance < equippedSkill.rangeMin || distance > equippedSkill.rangeMax + stats.currentPO)
            return;

        TileCoord targetTile = gridManager.TileMap[targetCoord].GetComponent<TileCoord>();

        if (equippedSkill.impactZone.zone.Length == 1)
        {
            if (targetTile.occupant == null)
                return;

            if (equippedSkill.maxPerTargetPerTurn > 0)
            {
                if (!perTargetCastCount.ContainsKey(targetTile.occupant))
                    perTargetCastCount[targetTile.occupant] = 0;

                if (perTargetCastCount[targetTile.occupant] >= equippedSkill.maxPerTargetPerTurn)
                    return;
            }

            ApplySkill(targetTile.occupant);
            perTargetCastCount[targetTile.occupant]++;
            stats.currentPA -= equippedSkill.costPA;
        }
        else if (equippedSkill.impactZone.zone.Length > 1)
        {
            List<GameObject> targets = GetTargetsInImpactZone(targetCoord);
            if (targets.Count == 0)
                return;

            foreach (GameObject target in targets)
            {
                ApplySkill(target);
            }

            stats.currentPA -= equippedSkill.costPA;
        }
    }

    private Vector2Int GetCurrentCoord()
    {
        Vector3 pos = transform.position;
        float minDist = float.MaxValue;
        Vector2Int closest = Vector2Int.zero;

        foreach (var kvp in gridManager.TileMap)
        {
            float d = Vector3.Distance(pos, kvp.Value.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = kvp.Key;
            }
        }

        return closest;
    }

    private List<GameObject> GetTargetsInImpactZone(Vector2Int centerCoord)
    {
        List<GameObject> targets = new List<GameObject>();

        if (equippedSkill.impactZone == null || equippedSkill.impactZone.zone == null)
            return targets;

        foreach (Vector2Int offset in equippedSkill.impactZone.zone)
        {
            Vector2Int targetCoord = centerCoord + offset;

            if (gridManager.TileMap.TryGetValue(targetCoord, out GameObject tile))
            {
                TileCoord tileData = tile.GetComponent<TileCoord>();
                if (tileData != null && tileData.occupant != null)
                {
                    targets.Add(tileData.occupant);
                }
            }
        }

        return targets;
    }

    private void ApplySkill(GameObject target)
    {
        CombatStats targetStats = target.GetComponent<CombatStats>();
        if (targetStats == null) return;

        float critChance = stats.currentCritChance + equippedSkill.critChance;
        bool isCrit = critChance >= 100 || Random.value < (critChance / 100f);

        int jet = Random.Range(equippedSkill.damageMin, equippedSkill.damageMax + 1);

        float statMultiplier = (stats.GetStatForType(equippedSkill.skillElement) + 100f) / 100f;
        float resistance = targetStats.GetResistance(equippedSkill.skillElement);

        float damage = jet * statMultiplier;
        if (isCrit) damage *= 1.5f;
        damage *= (100f - resistance) / 100f;

        int finalDamage = Mathf.RoundToInt(damage);

        if (targetStats.currentShield > 0)
        {
            if (finalDamage >= targetStats.currentShield)
            {
                finalDamage -= targetStats.currentShield;
                targetStats.currentShield = 0;
            }
            else
            {
                targetStats.currentShield -= finalDamage;
                finalDamage = 0;
            }
        }

        targetStats.currentHP -= finalDamage;
        targetStats.VerifDead();

        CombatManager CM = FindAnyObjectByType<CombatManager>();
        CM.VerifTeamDead();

        foreach (SkillEffect effect in equippedSkill.effects)
        {
            CombatStats targetToAffect = effect.applyToSelf ? stats : targetStats;
            if (targetToAffect == null) continue;

            if (effect.duration > 0)
            {
                targetToAffect.activeEffects.Add(new ActiveEffect(effect));
            }
            else
            {
                targetToAffect.ApplyInstantEffect(effect);
            }
        }

        if (isCrit && equippedSkill.critEffects != null)
        {
            foreach (var effect in equippedSkill.critEffects)
            {
                if (effect.applyToSelf)
                {
                    ApplySkillEffect(effect, stats);
                }
                else
                {
                    ApplySkillEffect(effect, targetStats);
                }
            }
        }
    }

    private void ApplySkillEffect(SkillEffect effect, CombatStats targetStats)
    {
        int val = Mathf.RoundToInt(effect.value);

        switch (effect.effectType)
        {
            case EffectType.BonusPV: targetStats.currentHP += val; break;
            case EffectType.BonusPA: targetStats.currentPA += val; break;
            case EffectType.MalusPA: targetStats.currentPA -= val; break;
            case EffectType.BonusPM: targetStats.currentPM += val; break;
            case EffectType.MalusPM: targetStats.currentPM -= val; break;
            case EffectType.BonusPO: targetStats.currentPO += val; break;
            case EffectType.MalusPO: targetStats.currentPO -= val; break;
            case EffectType.BonusFor: targetStats.currentForce += val; break;
            case EffectType.MalusFor: targetStats.currentForce -= val; break;
            case EffectType.BonusDex: targetStats.currentDexterite += val; break;
            case EffectType.MalusDex: targetStats.currentDexterite -= val; break;
            case EffectType.BonusMag: targetStats.currentMagie += val; break;
            case EffectType.MalusMag: targetStats.currentMagie -= val; break;
            case EffectType.BonusFoi: targetStats.currentFoi += val; break;
            case EffectType.MalusFoi: targetStats.currentFoi -= val; break;
            case EffectType.BonusResFor: targetStats.currentResistanceForce += val; break;
            case EffectType.MalusResFor: targetStats.currentResistanceForce -= val; break;
            case EffectType.BonusResDex: targetStats.currentResistanceDexterite += val; break;
            case EffectType.MalusResDex: targetStats.currentResistanceDexterite -= val; break;
            case EffectType.BonusResMag: targetStats.currentResistanceMagie += val; break;
            case EffectType.MalusResMag: targetStats.currentResistanceMagie -= val; break;
            case EffectType.BonusResFoi: targetStats.currentResistanceFoi += val; break;
            case EffectType.MalusResFoi: targetStats.currentResistanceFoi -= val; break;
            default: break;
        }
    }

    public void ResetSkillTurnUsage()
    {
        perTargetCastCount.Clear();
    }

    public void SelectSkill(SkillData newSkill)
    {
        equippedSkill = newSkill;
    }
}
