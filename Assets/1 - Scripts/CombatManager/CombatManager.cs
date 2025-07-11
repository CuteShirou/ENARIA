using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEditor.EditorTools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

public class CombatManager : MonoBehaviour
{
    public List<GameObject> allies = new List<GameObject>();
    public List<GameObject> ennemies = new List<GameObject>();

    public static bool isCombatPhase = false;


    //public GameObject[] allies;
    //public GameObject[] ennemies;

    private bool turnStarted = false;

    public bool TeamGreenDead = false;
    public bool TeamRedDead = false;

    public List<GameObject> fighters = new List<GameObject>();
    private int currentTurnIndex = 0;

    public void RegisterFighter(GameObject go)
    {
        if (!fighters.Contains(go))
            fighters.Add(go);
    }



    public void InitCombat()
    {
        ////Plus de case brillante au premier tour
        //Grid GM = FindAnyObjectByType<Grid>();
        //GM.ClearOccupant();
        //GM.ClearGrid();

        TeamGreenDead = false;
        TeamRedDead = false;

        for (int i = 0; i < fighters.Count; i++)
        {
            var tpc = fighters[i].GetComponent<ThirdPersonController>();
            if (tpc != null) tpc.enabled = false;

            var gcc = fighters[i].GetComponent<CombatController>();
            if (gcc != null)
            {
                gcc.enabled = false;
                gcc.gridManager = FindAnyObjectByType<Grid>();
            }
            var sc = fighters[i].GetComponent<SkillCaster>();
            if (sc != null)
            {
                sc.enabled = false;
                sc.gridManager = FindAnyObjectByType<Grid>();
            }

            //Repartition des Equipes:

            if (fighters[i].tag == "Player")
            {
                fighters[i].GetComponent<CombatStats>().team = 0;
                allies.Add(fighters[i]);
            }

            else if (fighters[i].tag == "Monster")
            {
                fighters[i].GetComponent<CombatStats>().team = 1;
                ennemies.Add(fighters[i]);
            }

        }
        currentTurnIndex = 0;

        // R�initialise tous les mat�riaux des cases � "normal"
        Grid grid = FindAnyObjectByType<Grid>();
        foreach (var tileGO in grid.TileMap.Values)
        {
            TileCoord tc = tileGO.GetComponent<TileCoord>();
            tileGO.GetComponent<Renderer>().sharedMaterial = tc.normal;
            tc.currentMaterial = tc.normal;
        }

        isCombatPhase = true;

        StartTurn();
    }

    void Update()
    {

        //// Passage au tour suivant manuellement (remplace par bouton plus tard)
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    EndTurn();
        //}


        // 11/07
        for (int i = 0; i < fighters.Count; i++)
        {
            if (fighters[i].GetComponent <CombatStats>().isDead == true)
            {
                fighters.Remove(fighters[i]);
            }
        }
        //-------------

            if ((TeamGreenDead == true) || (TeamRedDead == true))
        {

            Debug.Log("UNE DES EQUIPES MORTE");

            GameObject player = GameObject.FindWithTag("Player");

            for (int i = 0; i < fighters.Count; i++)
            {
                if (fighters[i].tag != "Player")
                {
                    // Si WIN ? Gain d'xp par le monstre tuer ? 
                    // Sinon: LOSER
                    Destroy(fighters[i]);
                    fighters.RemoveAt(i);
                }
            }

            allies.Clear();
            ennemies.Clear();
            fighters.Clear();


            Debug.Log(" Green: "+ TeamGreenDead + " ------  Red : "+   TeamRedDead);

            TeamGreenDead = false;
            TeamRedDead = false;

            Debug.Log(" Green: " + TeamGreenDead + " ------  Red : " + TeamRedDead);


            player.GetComponent<CombatController>().enabled = false;
            player.GetComponent<SkillCaster>().enabled = false;

            Grid GM = FindAnyObjectByType<Grid>();
            GM.ClearOccupant();
            GM.ClearGrid();

            ThirdPersonController TPC = player.GetComponent<ThirdPersonController>();
            TPC.enabled = true;
            TPC.IsInCombat = false;

            CombatStats CB = player.GetComponent<CombatStats>();
            CB.currentHP = CB.baseHP;
            CB.isDead = false;

            EndFightingTeleport teleporteur = FindFirstObjectByType<EndFightingTeleport>();
            if (teleporteur != null)
            {
                StartCoroutine(teleporteur.SwitchSceneAdditive(player));
            }

        }

    }

    void StartTurn()
    {
        Grid grid = FindAnyObjectByType<Grid>();
        //grid.ClearGrid();

        GameObject fighter = fighters[currentTurnIndex];

        // R�initialise toutes les cases � normal
        foreach (var tileGO in grid.TileMap.Values)
        {
            tileGO.GetComponent<Renderer>().sharedMaterial = tileGO.GetComponent<TileCoord>().normal;
        }

        // Colore la case actuelle du combattant actif
        TileCoord tile = fighter.GetComponent<CombatController>()?.GetCurrentTile();
        if (tile != null)
        {
            tile.GetComponent<Renderer>().sharedMaterial = tile.activeFighter;
        }


        // Active le contr�leur de combat uniquement pour ce combattant
        for (int i = 0; i < fighters.Count; i++)
        {
            var controller = fighters[i].GetComponent<CombatController>();
            if (controller != null)
                controller.enabled = (i == currentTurnIndex);
            var sc = fighters[i].GetComponent<SkillCaster>();
            if (sc != null)
                sc.enabled = (i == currentTurnIndex);
        }

        Debug.Log("Tour de : " + fighter.name);

        CombatStats stats = fighters[currentTurnIndex].GetComponent<CombatStats>();
        if (stats != null)
        {
            stats.UpdateActiveEffects();
            
        }

        turnStarted = true;
    }

    public void EndTurn()
    {
        GameObject fighter = fighters[currentTurnIndex];
        if (fighter.GetComponent<CombatController>().isMoving == true)
        {
            return;
        }
        else
        {
            // R�initialise les PM / PA
            var stats = fighter.GetComponent<CombatStats>();
            if (stats != null)
                stats.ResetTurnStats();

            SkillCaster caster = fighters[currentTurnIndex].GetComponent<SkillCaster>();
            if (caster != null)
                caster.ResetSkillTurnUsage();

            currentTurnIndex = (currentTurnIndex + 1) % fighters.Count;
            StartTurn();
        }
    }

    public void VerifTeamDead()
    {
        // V�rifie si tous les alli�s sont morts
        bool allAlliesDead = true;
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].GetComponent<CombatStats>().isDead == false)
            {
                allAlliesDead = false;
                break;
            }
        }
        TeamGreenDead = allAlliesDead;

        // V�rifie si tous les ennemis sont morts
        bool allEnemiesDead = true;
        for (int i = 0; i < ennemies.Count; i++)
        {
            if (ennemies[i].GetComponent<CombatStats>().isDead == false)
            {
                allEnemiesDead = false;
                break;
            }
        }
        TeamRedDead = allEnemiesDead;
    }
}

