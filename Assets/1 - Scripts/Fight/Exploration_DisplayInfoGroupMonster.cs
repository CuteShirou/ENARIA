using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Exploration_DisplayInfoGroupMonster : MonoBehaviour
{
    [SerializeField] private TextMeshPro textDisplay;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Exploration_InfoGroupMonster groupTeleporter;

    private void Start()
    {
        //groupTeleporter = GetComponent<Exploration_InfoGroupMonster>();
        groupTeleporter = GetComponentInParent<Exploration_InfoGroupMonster>();
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (textDisplay == null || groupTeleporter == null) return;

        List<string> monsterLines = new List<string>();
        foreach (GameObject monster in groupTeleporter.monstersInGroup)
        {
            if (monster == null) continue;

            Data_Information_Entity infos = monster.GetComponent<Data_Information_Entity>();
            if (infos != null)
                monsterLines.Add($"{infos.entity_Name} (LVL {infos.entity_Level})");
            else
                monsterLines.Add($"{monster.name} (LVL ?)");
        }

        textDisplay.text = string.Join("\n", monsterLines);
        textDisplay.gameObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (textDisplay != null && textDisplay.gameObject.activeSelf)
        {
            textDisplay.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
