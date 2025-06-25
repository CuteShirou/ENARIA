using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class RegisterManager : MonoBehaviour
{
    public string registerURL = "https://enaria.nexus-com.fr/register.php";

    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public void OnRegisterClicked()
    {
        StartCoroutine(Register(usernameInput.text, emailInput.text, passwordInput.text));
    }

    public IEnumerator Register(string username, string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("email", email);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post(registerURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Compte créé avec succès !");
        }
        else
        {
            Debug.LogError("Erreur : " + www.error);
        }
    }
}