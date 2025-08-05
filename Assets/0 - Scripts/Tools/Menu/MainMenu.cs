using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerPasswordConfirmField;
    public TMP_InputField registerEmailField;

    public Transform characterButtonContainer;

    private string username;
    private string password;

    private string selectedSpecie = null;
    private Button selectedButton = null;

    private List<string> registeredUsernames = new List<string>();

    private string input;

    public void SceneGame()
    {
        SceneManager.LoadSceneAsync("World 1 - Hub");
        Debug.Log("Scene jeu");
    }

    public void SceneCharacterSelection()
    {
        SceneManager.LoadSceneAsync("CharSelection");
        Debug.Log("Scene sélection perso");
    }

    public void SceneCharacterCreation()
    {
        SceneManager.LoadSceneAsync("CharCreation");
        Debug.Log("Scene création perso");
    }

    public void SceneMenu()
    {
        SceneManager.LoadSceneAsync("ConexionMenu");
        Debug.Log("Scene Menu");
    }

    public void TestButton()
    {
        Debug.Log("Bouton de test fonctionnel");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReadStringInput(string s)
    {
        input = s;
        Debug.Log(input);
    }

    public void Connexion()
    {
        username = usernameField.text;
        password = passwordField.text;

        Debug.Log("Connexion : " + username);

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("Aucun nom d'utilisateur");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning("Aucun mot de passe");
            return;
        }

        if (username == "admin" && password == "1234")
        {
            SceneCharacterSelection();
        }
        else
        {
            Debug.LogWarning("Identifiants incorrects");
        }
    }

    public void Register()
    {
        string regUsername = registerUsernameField.text;
        string regPassword = registerPasswordField.text;
        string regPasswordConfirm = registerPasswordConfirmField.text;
        string regEmail = registerEmailField.text;

        if (string.IsNullOrWhiteSpace(regUsername) ||
            string.IsNullOrWhiteSpace(regPassword) ||
            string.IsNullOrWhiteSpace(regPasswordConfirm) ||
            string.IsNullOrWhiteSpace(regEmail))
        {
            Debug.LogWarning("Tous les champs doivent être remplis.");
            return;
        }

        if (regPassword != regPasswordConfirm)
        {
            Debug.LogWarning("Les mots de passe ne correspondent pas.");
            return;
        }

        if (registeredUsernames.Contains(regUsername))
        {
            Debug.LogWarning("Nom d'utilisateur déjà utilisé.");
            return;
        }

        registeredUsernames.Add(regUsername);
        Debug.Log($"Compte enregistré : {regUsername}, Email : {regEmail}");
    }


    public void SelectSpecies(string specieName, Button button)
    {
        selectedSpecie = specieName;
        selectedButton = button;

        Debug.Log("Espèce sélectionnée : " + selectedSpecie);

        RegisterSwap(button);
    }

    public void SelectSpeciesFromUI(string specieName)
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        SelectSpecies(specieName, button);
    }

    public void CreateCharacter()
    {
        string characterName = usernameField.text;

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogWarning("Aucun nom de perso.");
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedSpecie))
        {
            Debug.LogWarning("Aucune espèce sélectionnée.");
            return;
        }

        Debug.Log($"Personnage créé : Nom = {characterName}, Espèce = {selectedSpecie}");

        SceneCharacterSelection();
    }

    public void PlaySelectedCharacter()
    {
        if (string.IsNullOrWhiteSpace(selectedSpecie))
        {
            Debug.LogWarning("Perso non selectionner");
            return;
        }

        SceneGame();
    }


    private void RegisterSwap(Button clickedButton)
    {
        if (characterButtonContainer == null)
        {
            return;
        }

        float tolerance = 0.1f;
        Button centerButton = null;

        foreach (Transform child in characterButtonContainer)
        {
            Vector3 pos = child.localPosition;

            if (Mathf.Abs(pos.x - 75f) < tolerance && Mathf.Abs(pos.y + 75f) < tolerance)
            {
                centerButton = child.GetComponent<Button>();
                break;
            }
        }

        if (centerButton == null)
        {
            return;
        }

        if (clickedButton == centerButton)
        {
            return;
        }

        SwapButtons(clickedButton, centerButton);
    }


    private void SwapButtons(Button buttonA, Button buttonB)
    {
        Transform t1 = buttonA.transform;
        Transform t2 = buttonB.transform;

        Vector3 pos1 = t1.localPosition;
        Vector3 pos2 = t2.localPosition;

        t1.localPosition = pos2;
        t2.localPosition = pos1;

    }

}
