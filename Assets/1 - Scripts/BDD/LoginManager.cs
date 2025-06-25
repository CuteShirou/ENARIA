using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public string loginURL = "https://enaria.nexus-com.fr/login.php";

    public void OnLoginButtonClicked()
    {
        StartCoroutine(Login(usernameInput.text, passwordInput.text));
    }

    IEnumerator Login(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post(loginURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text == "success")
        {
            Debug.Log("Connexion réussie !");
            // charge la scène du jeu ou dashboard
        }
        else
        {
            Debug.LogError("Échec de la connexion !");
        }
    }
}