using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class CombatManager : MonoBehaviour
{
    public GameObject[] allies;
    public GameObject[] ennemies;
    private bool turnStarted = false;

    public List<GameObject> fighters = new List<GameObject>();
    private int currentTurnIndex = 0;

    public void RegisterFighter(GameObject go)
    {
        if (!fighters.Contains(go))
            fighters.Add(go);
    }

    public void InitCombat()
    {
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
        }

        currentTurnIndex = 0;
        StartTurn();
    }

    void Update()
    {
        // Passage au tour suivant manuellement (remplace par bouton plus tard)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    void StartTurn()
    {
        GameObject fighter = fighters[currentTurnIndex];

        // Active le contrôleur de combat uniquement pour ce combattant
        for (int i = 0; i < fighters.Count; i++)
        {
            var controller = fighters[i].GetComponent<CombatController>();
            if (controller != null)
                controller.enabled = (i == currentTurnIndex);
        }

        // Réinitialise les PM / PA
        var stats = fighter.GetComponent<CombatStats>();
        if (stats != null)
            stats.ResetTurnStats();

        Debug.Log("Tour de : " + fighter.name);
        turnStarted = true;
    }

    void EndTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % fighters.Count;
        StartTurn();
    }
}
