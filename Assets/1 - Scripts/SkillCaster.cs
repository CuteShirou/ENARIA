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
                if (distance < equippedSkill.rangeMin || distance > equippedSkill.rangeMax + stats.PO)
                {
                    Debug.Log("Hors de portée !");
                    return;
                }

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

        float critChance = stats.critChance + equippedSkill.critChance;
        bool isCrit;
        if (critChance < 100)
        {
            isCrit = Random.value < (stats.critChance / 100f);
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
        targetStats.currentHP -= finalDamage;

        string log = $"{name} lance {equippedSkill.skillName} sur {target.name} pour {finalDamage} dégâts";
        if (isCrit) log += " CRITIQUE !";
        Debug.Log(log);

        // Application d’un effet de critique (bonus/malus)
        if (isCrit && equippedSkill.critEffect != null && equippedSkill.critEffect.effectType != EffectType.Aucun)
        {
            ApplySkillCritEffect(equippedSkill.critEffect, targetStats);
        }
    }

    private void ApplySkillCritEffect(SkillEffect effect, CombatStats targetStats)
    {
        switch (effect.effectType)
        {
            case EffectType.BonusPA:
                targetStats.currentPA += Mathf.RoundToInt(effect.value);
                Debug.Log($" Effet critique : +{effect.value} PA à {targetStats.name}");
                break;

            case EffectType.MalusPM:
                targetStats.currentPM -= Mathf.RoundToInt(effect.value);
                Debug.Log($" Effet critique : -{effect.value} PM à {targetStats.name}");
                break;

            // Ajoute d'autres effets si tu veux

            default:
                Debug.Log("Effet critique inconnu ou non géré.");
                break;
        }
    }

    public void ResetSkillTurnUsage()
    {
        perTargetCastCount.Clear();
    }

}
