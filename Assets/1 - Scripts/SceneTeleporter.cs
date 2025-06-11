using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTeleporter : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Drag & Drop ici ta scène cible")]
    [SerializeField] private SceneAsset sceneToLoad;
#endif

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
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(SwitchSceneAdditive());
        }
    }

    private System.Collections.IEnumerator SwitchSceneAdditive()
    {
        Scene currentScene = gameObject.scene;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        // Optionnel : définir la scène cible comme active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        // Décharge uniquement la scène du téléporteur (pas les autres !)
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
            yield return null;
    }
}