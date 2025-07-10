using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class AnalyseDataFighting : MonoBehaviour
{
    public static AnalyseDataFighting Instance;

    public List<EntityData> teamRed = new();
    public List<EntityData> teamGreen = new();

    private int nextID = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int RegisterEntity(GameObject entity, TeamType team, Vector2Int startPos, int hp, CombatStats stats)
    {
        EntityData data = new EntityData
        {
            id = nextID++,
            team = team,
            reference = entity,
            position = startPos,
            currentHP = hp,
            level = stats.baseInitiative,

            resistanceForce = stats.currentResistanceForce,
            resistanceDex = stats.currentResistanceDexterite,
            resistanceMagie = stats.currentResistanceMagie,
            resistanceFoi = stats.currentResistanceFoi
        };

        if (team == TeamType.Green)
            teamGreen.Add(data);
        else
            teamRed.Add(data);

        return data.id;
    }


    public void UpdateEntity(int id, Vector2Int? newPosition = null, int? newHP = null)
    {
        foreach (var list in new[] { teamGreen, teamRed })
        {
            foreach (var entity in list)
            {
                if (entity.id == id)
                {
                    if (newPosition.HasValue) entity.position = newPosition.Value;
                    if (newHP.HasValue) entity.currentHP = newHP.Value;
                    return;
                }
            }
        }
    }

    public EntityData GetEntityByID(int id)
    {
        foreach (var list in new[] { teamGreen, teamRed })
        {
            foreach (var entity in list)
            {
                if (entity.id == id)
                    return entity;
            }
        }
        return null;
    }
}
