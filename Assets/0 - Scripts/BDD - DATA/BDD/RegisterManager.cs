using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RegisterManager : MonoBehaviour
{
    public string registerURL = "https://enaria.nexus-com.fr/register.php";

    public TMP_InputField emailInput;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    public string loginSceneName = "LoginScene"; // Nom de la scène de login à charger après succès

    public void OnRegisterClicked()
    {
        // Force la fin d'édition du champ sélectionné
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        string email = emailInput.text;
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log("<color=cyan>Final check</color>");
        Debug.Log("Email = '" + email + "'");
        Debug.Log("Username = '" + username + "'");
        Debug.Log("Password = '" + password + "'");

        // ✅ Ordre corrigé : email, username, password
        StartCoroutine(Register(email, username, password));
    }

    public IEnumerator Register(string email, string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("username", username);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post(registerURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string response = www.downloadHandler.text.Trim().ToLower();

            if (response == "success")
            {
                Debug.Log("Compte créé avec succès !");
                messageText.text = "<color=green>Compte créé avec succès !</color>";
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene(loginSceneName);
            }
            else
            {
                Debug.LogWarning("Réponse serveur inattendue : " + response);
                messageText.text = "<color=red>Erreur serveur : " + response + "</color>";
            }
        }
        else
        {
            Debug.LogError("Erreur réseau : " + www.error);
            messageText.text = "<color=red>Erreur réseau : " + www.error + "</color>";
        }
    }
}
