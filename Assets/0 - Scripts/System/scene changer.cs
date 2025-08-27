using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneChanger : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, Tooltip("Glisser-déposer l’asset de scène ici. (Éditeur uniquement)")]
    private SceneAsset sceneAsset;
#endif

    private string sceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif

    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneChanger] Aucun asset de scène assigné dans l’inspecteur.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
