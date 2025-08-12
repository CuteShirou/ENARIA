using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationManager : MonoBehaviour
{
    [Header("Noms de scènes (à configurer dans l'inspecteur)")]
    public string loginScene = "Login";
    public string registerScene = "Register";
    public string charSelectionScene = "CharSelection";
    public string charCreationScene = "CharCreation";

    [Header("Optionnel")]
    [Tooltip("Temps minimum d'affichage (utile si tu veux un petit délai/animation)")]
    public float minLoadDelay = 0f;

    public void GoToLogin() => StartCoroutine(LoadSceneRoutine(loginScene));
    public void GoToRegister() => StartCoroutine(LoadSceneRoutine(registerScene));
    public void GoToCharSelection()
    {
        bool isLogged = PlayerPrefs.GetInt("isLoggedIn", 0) == 1;

        if (isLogged)
        {
            StartCoroutine(LoadSceneRoutine(charSelectionScene));
        }
        else
        {
            Debug.LogWarning("NavigationManager: accès à CharSelection refusé — utilisateur non connecté. Redirection vers Login.");
            StartCoroutine(LoadSceneRoutine(loginScene));
        }
    }
    public void GoToCharCreation() => StartCoroutine(LoadSceneRoutine(charCreationScene));

    public void GoTo(string sceneName) => StartCoroutine(LoadSceneRoutine(sceneName));

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("NavigationManager: nom de scène vide.");
            yield break;
        }

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"NavigationManager: la scène '{sceneName}' n'est pas ajoutée aux Build Settings.");
            yield break;
        }

        if (minLoadDelay > 0f)
            yield return new WaitForSeconds(minLoadDelay);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("isLoggedIn");
        PlayerPrefs.DeleteKey("loggedUsername");
        PlayerPrefs.Save();
        Debug.Log("NavigationManager: utilisateur déconnecté (PlayerPrefs nettoyé).");

        StartCoroutine(LoadSceneRoutine(loginScene));
    }
}
