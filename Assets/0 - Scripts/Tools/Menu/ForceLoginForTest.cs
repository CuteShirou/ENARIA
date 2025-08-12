using UnityEngine;

public class ForceLoginForTest : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetInt("isLoggedIn", 1);
        PlayerPrefs.SetString("loggedUsername", "TestUser");
        PlayerPrefs.Save();
        Debug.Log("ForceLoginForTest : connexion forcée pour tests.");
    }
}
