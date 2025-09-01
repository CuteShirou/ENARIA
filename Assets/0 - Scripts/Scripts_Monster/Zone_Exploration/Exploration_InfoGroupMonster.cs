using System.Collections.Generic;
using UnityEngine;

// Représente l'état du groupe de monstres
public enum MonsterState
{
    InNature,   // Peut être attaqué
    Attacked,   // A été engagé
    InFight     // En combat actif
}
//------------------------------------------
public class Exploration_InfoGroupMonster : MonoBehaviour
{
    [Tooltip("Nom Du Groupe:")]
    public string groupName = "Groupe de monstres";

    [Header("Groupe de monstres à instancier dans CombatScene")]
    public List<GameObject> monstersInGroup = new();

    [Header("Grille à utiliser pour ce combat")]
    public Data_FightMap combatMap;

    [Header("Arène associé")]
    public int arenaIndex = 0;

    [Header("Caméra Exploration associé")]
    public string cameraExplo = "";

    // État actuel du groupe de monstres
    [Header("État du groupe de monstres")]
    public MonsterState currentState = MonsterState.InNature;

    // Change l'état du monstre
    public void SetState(MonsterState newState)
    {
        currentState = newState;
        Debug.Log($"{groupName} passe à l'état {newState}");
    }

    // Retourne true si le monstre est dans l'état donné
    public bool IsState(MonsterState stateToCheck)
    {
        return currentState == stateToCheck;
    }
}
