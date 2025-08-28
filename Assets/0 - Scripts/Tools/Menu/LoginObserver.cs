using UnityEngine;
using TMPro;

public class LoginObserver : MonoBehaviour
{
    [Tooltip("Référence au TMP_Text que LoginManager met à jour")]
    public TMP_Text messageText;

    [Tooltip("Optionnel : le champ utilisateur pour sauvegarder le pseudo")]
    public TMP_InputField usernameOrEmailInput;

    [Tooltip("Texte indiquant la réussite (case-insensitive).")]
    public string successIndicator = "connexion réussie";

    private bool alreadySet = false;

    void Update()
    {
        if (messageText == null) return;
        if (alreadySet) return;

        string msg = messageText.text?.ToLower() ?? "";

        if (msg.Contains(successIndicator.ToLower()))
        {
            PlayerPrefs.SetInt("isLoggedIn", 1);

            if (usernameOrEmailInput != null)
            {
                PlayerPrefs.SetString("loggedUsername", usernameOrEmailInput.text ?? "");
            }

            PlayerPrefs.Save();
            alreadySet = true;
            Debug.Log("LoginObserver: flag isLoggedIn = 1 sauvegardé dans PlayerPrefs.");
        }
    }

    public void ResetFlag()
    {
        alreadySet = false;
        PlayerPrefs.DeleteKey("isLoggedIn");
        PlayerPrefs.DeleteKey("loggedUsername");
        PlayerPrefs.Save();
        Debug.Log("LoginObserver: flag de connexion réinitialisé.");
    }
}
