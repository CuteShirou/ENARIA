using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [Tooltip("Référence à l'UI existante dans la scène (désactivée par défaut)")]
    public GameObject InventoryPage;
    public GameObject ButtonInteraction;
    
    private void Awake()
    {
        if (InventoryPage != null)
            InventoryPage.SetActive(false); // Assure que l'UI est cachée au démarrage
        
        if (ButtonInteraction != null)
            ButtonInteraction.SetActive(false); // Assure que l'UI est cachée au démarrage
    }

    public void Toggle()
    {
        Debug.Log("Appel depuis : " + gameObject.name);

        if (InventoryPage == null)
            Debug.LogWarning("InventoryPage n'est pas assigné !");
        if (ButtonInteraction == null)
            Debug.LogWarning("ButtonInteraction n'est pas assigné !");

        if (InventoryPage == null || ButtonInteraction == null)
            return;

        bool isActive = InventoryPage.activeSelf;

        InventoryPage.SetActive(!isActive);
        ButtonInteraction.SetActive(!isActive);

        Debug.Log("UI " + (!isActive ? "activée." : "désactivée."));
    }
}