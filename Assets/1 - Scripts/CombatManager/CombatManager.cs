using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using StarterAssets;

public class CombatManager : MonoBehaviour
{
    public GameObject[] allies;
    public GameObject[] ennemies;
    public List<GameObject> fighters;
    int turnorder = 1;
    // Start is called before the first frame update
    void Start()
    {
        allies = GameObject.FindGameObjectsWithTag("Player");
        //ennemies = GameObject.FindGameObjectsWithTag("Monster");
        fighters.AddRange(allies);
        //fighters.AddRange(ennemies);
        for (int i = 0; i < fighters.Count; i++)
        {
            fighters[i].GetComponent<ThirdPersonController>()._turnOrder = turnorder;
            turnorder++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
