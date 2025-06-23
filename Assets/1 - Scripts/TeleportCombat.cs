using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using UnityEngine.Animations;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TeleportCombat : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Drag & Drop ici ta scène cible")]
    [SerializeField] private SceneAsset sceneToLoad;
#endif

    [Header("Transform cible en fallback si pas de grille trouvée")]
    [SerializeField] private Transform fallbackTransform;

    [Header("Nom du parent de caméra à activer (ParentConstraint)")]
    [SerializeField] private string cameraParentTargetName;
    [SerializeField] private string playerTag = "Player";

    [HideInInspector]
    public string sceneName;

    [Header("Combat")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int numberOfMonsters = 1;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneToLoad != null)
            sceneName = sceneToLoad.name;
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(SwitchSceneAndPlaceOnGrid(other.gameObject));
            other.GetComponent<ThirdPersonController>()._isInCombat = true;
        }
    }

    private IEnumerator SwitchSceneAndPlaceOnGrid(GameObject player)
    {
        Scene currentScene = gameObject.scene;

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

            // 1. Positionner le joueur 
            Vector2Int playerCoord = GetAndRemoveRandomCoord(ref availableTiles);
            GameObject playerTile = gridManager.TileMap[playerCoord];
            player.transform.position = playerTile.transform.position + new Vector3(0, 0.5f, 0);
            player.transform.rotation = Quaternion.identity;
            TileCoord tileCoord = playerTile.GetComponent<TileCoord>();
                //définis que la case est occupée par le joueur
            if (tileCoord != null)
                tileCoord.SetOccupant(player);

            // 2. Instancier les monstres
            List<GameObject> spawnedMonsters = new List<GameObject>();
            for (int i = 0; i < numberOfMonsters; i++)
            {
                Vector2Int monsterCoord = GetAndRemoveRandomCoord(ref availableTiles);
                GameObject tile = gridManager.TileMap[monsterCoord];

                GameObject monster = Instantiate(monsterPrefab, tile.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                TileCoord monsterTileCoord = tile.GetComponent<TileCoord>();
                    //définis que la case est occupée par le monstre
                if (monsterTileCoord != null)
                    monsterTileCoord.SetOccupant(monster);

                monster.tag = "Monster"; // Assure que le tag est correct
                monster.name = $"Monster {i + 1}";
                spawnedMonsters.Add(monster);
            }

            // 3. Enregistrement dans le CombatManager
            CombatManager cm = FindAnyObjectByType<CombatManager>();
            if (cm != null)
            {
                cm.RegisterFighter(player);
                foreach (var monster in spawnedMonsters)
                    cm.RegisterFighter(monster);

                cm.InitCombat(); // Lance le tour par tour
            }


        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
    }

    // Méthode utilitaire
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
            Debug.LogWarning("SceneTeleporter: MainCamera not found.");
            return;
        }

        ParentConstraint constraint = mainCam.GetComponent<ParentConstraint>();
        if (constraint == null)
        {
            Debug.LogWarning("SceneTeleporter: ParentConstraint not found on MainCamera.");
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
