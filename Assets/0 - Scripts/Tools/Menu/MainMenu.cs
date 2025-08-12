using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Character creation")]
    public TMP_InputField characterNameField;
    public Transform characterButtonContainer;
    public Button createButton;                // le bouton "Créer" du UI (à assigner)
    public TMP_Text feedbackText;              // optionnel : pour afficher warnings

    private string selectedSpecie = null;
    private Button selectedButton = null;

    private NavigationManager nav;

    private void Awake()
    {
        nav = FindNavigationManager();
    }

    private void Start()
    {
        // On s'assure que le bouton Create appelle uniquement CreateCharacter()
        if (createButton != null)
        {
            createButton.onClick.RemoveAllListeners();
            createButton.onClick.AddListener(CreateCharacter);
        }

        // Écoute les changements de texte pour activer/désactiver le bouton
        if (characterNameField != null)
            characterNameField.onValueChanged.AddListener(_ => UpdateCreateButtonState());

        UpdateCreateButtonState();
    }

    public void TestButton() => Debug.Log("Bouton de test fonctionnel");

    public void QuitGame() => Application.Quit();

    public void SelectSpecies(string specieName, Button button)
    {
        selectedSpecie = specieName;
        selectedButton = button;

        Debug.Log("Espèce sélectionnée : " + selectedSpecie);

        RegisterSwap(button);
        UpdateCreateButtonState();
    }

    public void SelectSpeciesFromUI(string specieName)
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        SelectSpecies(specieName, button);
    }

    public void CreateCharacter()
    {
        string characterName = characterNameField != null ? characterNameField.text : "";

        if (string.IsNullOrWhiteSpace(characterName))
        {
            ShowFeedback("Aucun nom de perso.");
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedSpecie))
        {
            ShowFeedback("Aucune espèce sélectionnée.");
            return;
        }

        ClearFeedback();
        Debug.Log($"Personnage créé : Nom = {characterName}, Espèce = {selectedSpecie}");

        if (nav != null)
        {
            nav.GoToCharSelection();
        }
        else
        {
            Debug.LogWarning("NavigationManager introuvable — fallback vers CharSelection.");
            SceneManager.LoadSceneAsync("CharSelection");
        }
    }

    public void PlaySelectedCharacter()
    {
        if (string.IsNullOrWhiteSpace(selectedSpecie))
        {
            ShowFeedback("Perso non sélectionné");
            return;
        }

        if (nav != null)
        {
            nav.GoTo("World 1 - Hub");
        }
        else
        {
            Debug.LogWarning("NavigationManager introuvable — fallback vers World 1 - Hub.");
            SceneManager.LoadSceneAsync("World 1 - Hub");
        }
    }

    private void RegisterSwap(Button clickedButton)
    {
        if (characterButtonContainer == null || clickedButton == null)
            return;

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

        if (centerButton == null || clickedButton == centerButton)
            return;

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

    // Active/désactive le bouton create selon la validité
    private void UpdateCreateButtonState()
    {
        bool nameOk = !(characterNameField == null || string.IsNullOrWhiteSpace(characterNameField.text));
        bool specieOk = !string.IsNullOrWhiteSpace(selectedSpecie);
        if (createButton != null) createButton.interactable = nameOk && specieOk;
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = $"<color=red>{msg}</color>";
        else Debug.LogWarning(msg);
    }

    private void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }

    // Recherche NavigationManager (compatible versions Unity récentes/anciennes)
    private NavigationManager FindNavigationManager()
    {
#if UNITY_2023_2_OR_NEWER
        NavigationManager nm = UnityEngine.Object.FindFirstObjectByType<NavigationManager>();
        if (nm != null) return nm;
        return UnityEngine.Object.FindAnyObjectByType<NavigationManager>();
#else
        return FindObjectOfType<NavigationManager>();
#endif
    }
}
