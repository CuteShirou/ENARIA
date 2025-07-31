using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------
public class Phase_EnterSetupCombat : MonoBehaviour
{
    [Header("Carte utilisée pour ce combat")]
    public Data_FightMap combatMap;

    [Header("Équipes de combat")]
    public List<GameObject> greenTeam = new();   // Joueurs (équipe verte)
    public List<GameObject> redTeam = new();     // Monstres (équipe rouge)
    public List<GameObject> AllFighters = new(); // Ordre de tour (vert + rouge)

    // Groupe de monstres déclencheur du combat
    private Exploration_InfoGroupMonster currentGroup;

    // Référence vers le Combat_PhaseManager principal
    private Combat_PhaseManager manager;

    // Appelée par le Combat_PhaseManager lors de StartPhase(Enter)
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        Debug.Log($"[Phase_EnterSetupCombat][Arena {manager.arenaIndex}] Phase d'entrée activée.");
        Debug.Log($"[Phase_EnterSetupCombat] Joueurs en attente : {greenTeam.Count}");
        Debug.Log($"[Phase_EnterSetupCombat] Monstres dans le groupe : {redTeam.Count}");
        Debug.Log($"[Phase_EnterSetupCombat] Carte utilisée : {(combatMap != null ? combatMap.name : "aucune")}");

        // Transition automatique vers la phase suivante
        Debug.Log("[Phase_EnterSetupCombat] Phase terminée. Passage à la phase de préparation.");
        manager.NextPhase();
    }

    // Rempli les données de combat (appelé depuis Exploration_Trigger_GoCombat)
    public void SetCombatData(List<GameObject> newMonsters, Data_FightMap newMap, Exploration_InfoGroupMonster group)
    {
        redTeam = newMonsters;
        combatMap = newMap;
        currentGroup = group;

        AllFighters.AddRange(redTeam);

        Debug.Log("[Phase_EnterSetupCombat] Données de combat reçues.");
        Debug.Log($" - Monstres : {redTeam.Count}");
        Debug.Log($" - Carte : {(combatMap != null ? combatMap.name : "null")}");
    }

    // Ajoute un joueur dans l'équipe verte
    public void AddPlayerToTeamVerte(GameObject player)
    {
        if (!greenTeam.Contains(player))
        {
            greenTeam.Add(player);
            AllFighters.Add(player);
            Debug.Log("[Phase_EnterSetupCombat] Joueur ajouté à l'équipe verte : " + player.name);
        }
        else
        {
            Debug.Log("[Phase_EnterSetupCombat] Joueur déjà dans l'équipe verte : " + player.name);
        }
    }

    // Permet aux autres phases ou au manager de changer l'état du groupe
    public void SetMonsterState(MonsterState newState)
    {
        if (currentGroup != null)
        {
            currentGroup.SetState(newState);
            Debug.Log("[Phase_EnterSetupCombat] État du groupe mis à jour : " + newState);
        }
        else
        {
            Debug.LogWarning("[Phase_EnterSetupCombat] Aucun groupe de monstres référencé.");
        }
    }
}
