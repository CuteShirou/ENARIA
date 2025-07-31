using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [Tooltip("Référence à l'UI existante dans la scène (désactivée par défaut)")]
    public GameObject uiObject;

    private void Awake()
    {
        if (uiObject != null)
            uiObject.SetActive(false); // Assure que l'UI est cachée au démarrage
    }

    public void Toggle()
    {
        if (uiObject == null)
        {
            Debug.LogWarning("Aucun UI object n'est assigné.");
            return;
        }

        bool isActive = uiObject.activeSelf;
        uiObject.SetActive(!isActive);

        Debug.Log("UI " + (isActive ? "désactivée." : "activée."));
    }
}