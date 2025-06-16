using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTeleporter : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Drag & Drop ici ta scène cible")]
    [SerializeField] private SceneAsset sceneToLoad;
#endif

    [Header("Transform cible dans la scène à charger (destination du joueur)")]
    [SerializeField] private Transform destinationTransform;

    [Header("Nom du parent de caméra à activer (ParentConstraint)")]
    [SerializeField] private string cameraParentTargetName;

    [SerializeField] private string playerTag = "Player";

    [HideInInspector]
    public string sceneName;

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
            StartCoroutine(SwitchSceneAdditive(other.gameObject));
        }
    }

    private System.Collections.IEnumerator SwitchSceneAdditive(GameObject player)
    {
        Scene currentScene = gameObject.scene;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        // 1. Téléportation du joueur
        if (destinationTransform != null)
        {
            player.transform.position = destinationTransform.position;
            player.transform.rotation = destinationTransform.rotation;
        }

        // 2. Activation dynamique du bon parent caméra
        SetCameraParentByName(cameraParentTargetName);

        // 3. Déchargement de la scène actuelle
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
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
