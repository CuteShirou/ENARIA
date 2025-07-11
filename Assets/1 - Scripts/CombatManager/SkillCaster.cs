using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillCaster : MonoBehaviour
{
    public SkillData equippedSkill; // Le sort actuellement sélectionné
    public Grid gridManager;

    private CombatStats stats;
    private Camera mainCamera;

    private Dictionary<GameObject, int> perTargetCastCount = new Dictionary<GameObject, int>();
    private void Start()
    {
        stats = GetComponent<CombatStats>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (equippedSkill == null || stats == null || stats.currentPA < equippedSkill.costPA)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                TileCoord tile = hit.collider.GetComponent<TileCoord>();
                if (tile == null) return;

                Vector2Int from = GetCurrentCoord();
                Vector2Int to = tile.Coord;

                int distance = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
                if (distance < equippedSkill.rangeMin || distance > equippedSkill.rangeMax + stats.currentPO)
                {
                    Debug.Log("Hors de portée !");
                    return;
                }

                if (equippedSkill.impactZone.zone.Length == 1)
                {
                    if (tile.occupant == null)
                    {
                        Debug.Log("Aucune cible !");
                        return;
                    }

                    if (equippedSkill.maxPerTargetPerTurn > 0)
                    {
                        if (!perTargetCastCount.ContainsKey(tile.occupant))
                            perTargetCastCount[tile.occupant] = 0;

                        if (perTargetCastCount[tile.occupant] >= equippedSkill.maxPerTargetPerTurn)
                        {
                            Debug.LogWarning($" Tu as déjà lancé {equippedSkill.skillName} {perTargetCastCount[tile.occupant]}x sur {tile.occupant.name} ce tour !");
                            return;
                        }
                    }
                    ApplySkill(tile.occupant);
                    perTargetCastCount[tile.occupant]++;
                    stats.currentPA -= equippedSkill.costPA;
                }
                else if (equippedSkill.impactZone.zone.Length > 1)
                {
                    List<GameObject> targets = GetTargetsInImpactZone(tile.Coord);
                    if (targets.Count == 0)
                    {
                        Debug.Log("Aucune cible dans la zone d'impact !");
                        return;
                    }
                    else
                    {
                        Debug.Log($"Cible(s) trouvée(s) : {targets.Count} dans la zone d'impact.");
                        foreach (GameObject target in targets)
                        {
                            ApplySkill(target);
                        }
                    }
                    stats.currentPA -= equippedSkill.costPA;
                }
            }
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

    private void ApplySkill(GameObject target)
    {
        CombatStats targetStats = target.GetComponent<CombatStats>();
        if (targetStats == null) return;

        float critChance = stats.currentCritChance + equippedSkill.critChance;
        bool isCrit;
        if (critChance < 100)
        {
            isCrit = Random.value < (stats.currentCritChance / 100f);
        }
        else
        {
            isCrit = true;
        }

        int jet = Random.Range(equippedSkill.damageMin, equippedSkill.damageMax + 1);

        float statMultiplier = (stats.GetStatForType(equippedSkill.skillType) + 100f) / 100f;
        float resistance = targetStats.GetResistance(equippedSkill.skillType);

        float damage = jet * statMultiplier;

        if (isCrit)
        {
            damage *= 1.5f; // valeur arbitraire
        }

        damage *= (100f - resistance) / 100f;

        int finalDamage = Mathf.RoundToInt(damage);

        if (targetStats.currentShield > 0)
        {
            // Si la cible a un bouclier, on applique les dégâts au bouclier d'abord
            if (finalDamage >= targetStats.currentShield)
            {
                finalDamage -= targetStats.currentShield;
                targetStats.currentShield = 0;
            }
            else
            {
                targetStats.currentShield -= finalDamage;
                finalDamage = 0; // Pas de dégâts restants à appliquer aux PV
            }
        }
        targetStats.currentHP -= finalDamage;

        string log = $"{name} lance {equippedSkill.skillName} sur {target.name} pour {finalDamage} dégâts";
        if (isCrit) log += " CRITIQUE !";
        Debug.Log(log);

        // Application d’un effet (bonus/malus)
        foreach (SkillEffect effect in equippedSkill.effects)
        {
            CombatStats targetToAffect = effect.applyToSelf ? stats : target.GetComponent<CombatStats>();
            if (targetToAffect == null) continue;

            if (effect.duration > 0)
            {
                targetToAffect.activeEffects.Add(new ActiveEffect(effect));
                Debug.Log($" Effet temporaire {effect.effectType} ({effect.value}) pour {effect.duration} tour(s) sur {targetToAffect.name}");
            }
            else
            {
                // Appliquer effet simple, immédiat et sans suivi
                targetToAffect.ApplyInstantEffect(effect);
                Debug.Log($" Effet instantané {effect.effectType} ({effect.value}) appliqué sur {targetToAffect.name}");
            }
        }

        // Application d’un effet de critique (bonus/malus)
        if (isCrit && equippedSkill.critEffects != null)
        {
            foreach (var effect in equippedSkill.critEffects)
            {
                if (effect.applyToSelf)
                {
                    ApplySkillCritEffect(effect, stats);
                }
                else
                {
                    ApplySkillCritEffect(effect, targetStats);
                }
            }
        }
    }

    private void ApplySkillCritEffect(SkillEffect effect, CombatStats targetStats)
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

            default:
                Debug.Log("Effet critique inconnu ou non géré.");
                break;
        }
    }
    private void ApplySkillEffect(SkillEffect effect, CombatStats targetStats)
    {
        int val = Mathf.RoundToInt(effect.value);

        switch (effect.effectType)
        {
            case EffectType.BonusPA: targetStats.currentPA += val; break;
            case EffectType.MalusPA: targetStats.currentPA -= val; break;
            case EffectType.BonusPM: targetStats.currentPM += val; break;
            case EffectType.MalusPM: targetStats.currentPM -= val; break;

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

            default:
                Debug.LogWarning("Effet non pris en charge : " + effect.effectType);
                break;
        }

        Debug.Log($" Effet {effect.effectType} de {val} appliqué à {targetStats.name}");
    }

    public void ResetSkillTurnUsage()
    {
        perTargetCastCount.Clear();
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

}
