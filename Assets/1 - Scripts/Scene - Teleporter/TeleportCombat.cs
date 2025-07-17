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
            other.GetComponent<ThirdPersonController>().IsInCombat = true;
        }
    }

    private IEnumerator SwitchSceneAndPlaceOnGrid(GameObject player)
    {
        Scene currentScene = gameObject.scene;

        // Charger la scène de combat
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        // Activer la nouvelle scène
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        yield return null;

        // Récupérer le GridManager dans la scène cible
        Grid gridManager = null;
        foreach (GameObject root in newScene.GetRootGameObjects())
        {
            gridManager = root.GetComponentInChildren<Grid>();
            if (gridManager != null) break;
        }

        SetCameraParentByName(cameraParentTargetName);

        if (gridManager != null && gridManager.TileMap.Count > 0)
        {
            // 1. Instancier les monstres dans la bonne scène
            for (int i = 0; i < numberOfMonsters; i++)
            {
                GameObject monster = Instantiate(monsterPrefab, gridManager.transform);
                monster.tag = "Monster";
                monster.name = $"Monster {i + 1}";
            }

            // 2. Enregistrer les entités dans le CombatManager
            CombatManager cm = FindAnyObjectByType<CombatManager>();
            if (cm != null)
            {
                cm.RegisterFighter(player);

                foreach (var monster in GameObject.FindGameObjectsWithTag("Monster"))
                    cm.RegisterFighter(monster);
            }

            // 3. Lancer la phase de préparation
            CombatPreparationManager prep = FindAnyObjectByType<CombatPreparationManager>();
            if (prep != null)
            {
                prep.ForceSetup(); // C'est ici que les monstres sont placés sur les cases rouges
            }
        }

        // Décharger la scène précédente
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
    }

    // Méthode utilitaire : récupère une coordonnée aléatoire
    private Vector2Int GetAndRemoveRandomCoord(ref List<Vector2Int> coords)
    {
        int index = Random.Range(0, coords.Count);
        Vector2Int coord = coords[index];
        coords.RemoveAt(index);
        return coord;
    }

    // Méthode utilitaire : change le parent de la caméra
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
