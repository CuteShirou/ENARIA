using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;

public class TeleportCombat : MonoBehaviour
{
    [Header("Nom EXACT de la scène de combat (sans .unity)")]
    [SerializeField] private string sceneName;

    [Header("Transform cible en fallback si pas de grille trouvée")]
    [SerializeField] private Transform fallbackTransform;

    [Header("Nom du parent de caméra à activer (ParentConstraint)")]
    [SerializeField] private string cameraParentTargetName;
    [SerializeField] private string playerTag = "Player";

    [Header("Combat")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int numberOfMonsters = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(SwitchSceneAndPlaceOnGrid(other.gameObject));

            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = true;
            }
        }
    }

    private IEnumerator SwitchSceneAndPlaceOnGrid(GameObject player)
    {
        Scene currentScene = gameObject.scene;
        Debug.Log($"[TeleportCombat] Loading combat scene: {sceneName}");

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        yield return null;

        Grid gridManager = null;
        foreach (GameObject root in newScene.GetRootGameObjects())
        {
            gridManager = root.GetComponentInChildren<Grid>();
            if (gridManager != null) break;
        }

        SetCameraParentByName(cameraParentTargetName);

        if (gridManager != null && gridManager.TileMap.Count > 0)
        {
            List<Vector2Int> availableTiles = new List<Vector2Int>(gridManager.TileMap.Keys);

            Vector2Int playerCoord = GetAndRemoveRandomCoord(ref availableTiles);
            GameObject playerTile = gridManager.TileMap[playerCoord];
            player.transform.position = playerTile.transform.position + new Vector3(0, 0.5f, 0);
            player.transform.rotation = Quaternion.identity;
            TileCoord tileCoord = playerTile.GetComponent<TileCoord>();
            if (tileCoord != null)
                tileCoord.SetOccupant(player);

            List<GameObject> spawnedMonsters = new List<GameObject>();
            for (int i = 0; i < numberOfMonsters; i++)
            {
                Vector2Int monsterCoord = GetAndRemoveRandomCoord(ref availableTiles);
                GameObject tile = gridManager.TileMap[monsterCoord];

                GameObject monster = Instantiate(monsterPrefab, tile.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                TileCoord monsterTileCoord = tile.GetComponent<TileCoord>();
                if (monsterTileCoord != null)
                    monsterTileCoord.SetOccupant(monster);

                monster.tag = "Monster";
                monster.name = $"Monster {i + 1}";
                spawnedMonsters.Add(monster);
            }

            CombatManager cm = FindAnyObjectByType<CombatManager>();
            if (cm != null)
            {
                cm.RegisterFighter(player);
                foreach (var monster in spawnedMonsters)
                    cm.RegisterFighter(monster);
                cm.InitCombat();
            }
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
    }

    private Vector2Int GetAndRemoveRandomCoord(ref List<Vector2Int> coords)
    {
        int index = Random.Range(0, coords.Count);
        Vector2Int coord = coords[index];
        coords.RemoveAt(index);
        return coord;
    }

    private void SetCameraParentByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return;

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("TeleportCombat: MainCamera not found.");
            return;
        }

        ParentConstraint constraint = mainCam.GetComponent<ParentConstraint>();
        if (constraint == null)
        {
            Debug.LogWarning("TeleportCombat: ParentConstraint not found on MainCamera.");
            return;
        }

        for (int i = 0; i < constraint.sourceCount; i++)
        {
            ConstraintSource src = constraint.GetSource(i);
            src.weight = (src.sourceTransform.name == targetName) ? 1f : 0f;
            constraint.SetSource(i, src);
        }
    }
}
