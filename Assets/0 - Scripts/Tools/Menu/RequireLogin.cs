using UnityEngine;
using UnityEngine.SceneManagement;

public class RequireLogin : MonoBehaviour
{
    public string loginSceneName = "Login";

    void Awake()
    {
        bool isLogged = PlayerPrefs.GetInt("isLoggedIn", 0) == 1;
        if (!isLogged)
        {
            Debug.LogWarning("RequireLogin: accès refusé, redirection vers Login.");
            SceneManager.LoadScene(loginSceneName);
        }
        else
        {
            Debug.Log("RequireLogin: utilisateur connecté, accès autorisé.");
        }
    }
}
