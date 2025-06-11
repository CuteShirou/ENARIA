
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
            StartCoroutine(SwitchScene());
        }
    }

    private System.Collections.IEnumerator SwitchScene()
    {
        string currentScene = gameObject.scene.name;

        AsyncOperation unload = SceneManager.UnloadSceneAsync(currentScene);
        while (!unload.isDone)
            yield return null;

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!load.isDone)
            yield return null;
    }
}
