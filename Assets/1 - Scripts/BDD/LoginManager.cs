using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameOrEmailInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    public string loginURL = "https://enaria.nexus-com.fr/login.php";
    public string targetScene = "GameScene";

    public void OnLoginButtonClicked()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        string identifiant = usernameOrEmailInput.text;
        string password = passwordInput.text;

        Debug.Log("Tentative de connexion avec : " + identifiant);
        StartCoroutine(Login(identifiant, password));
    }

    IEnumerator Login(string identifiant, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("identifiant", identifiant);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post(loginURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string response = www.downloadHandler.text.Trim().ToLower();

            if (response == "success")
            {
                Debug.Log("Connexion réussie !");
                messageText.text = "<color=green>Connexion réussie !</color>";
                yield return new WaitForSeconds(1.5f);
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.LogWarning("Erreur de connexion : " + response);
                messageText.text = "<color=red>" + response + "</color>";
            }
        }
        else
        {
            Debug.LogError("Erreur réseau : " + www.error);
            messageText.text = "<color=red>Erreur réseau : " + www.error + "</color>";
        }
    }
}