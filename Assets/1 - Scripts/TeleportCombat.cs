
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TeleportCombat : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Drag & Drop ici ta sc�ne cible")]
    [SerializeField] private SceneAsset sceneToLoad;
#endif

    [Header("Transform cible dans la sc�ne � charger (destination du joueur)")]
    [SerializeField] private Transform destinationTransform;

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
            other.GetComponent<ThirdPersonController>()._isInCombat = true;
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

        // D�placement du joueur si une destination est sp�cifi�e
        if (destinationTransform != null)
        {
            player.transform.position = destinationTransform.position;
            player.transform.rotation = destinationTransform.rotation;
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
    }
}
