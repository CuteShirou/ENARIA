using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // Charge les scènes supplémentaires de manière additive
        SceneManager.LoadSceneAsync("Player", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("Zone Apparition", LoadSceneMode.Additive);
    }
}
