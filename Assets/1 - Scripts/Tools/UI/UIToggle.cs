using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [Tooltip("UI à activer/désactiver")]
    public GameObject targetUI;

    [Tooltip("Si activé, désactive les autres UIToggle sur la scène")]
    public bool uniqueToggle = false;

    void Start()
    {
        if (targetUI == null)
            Debug.LogWarning("UIToggle : Aucun targetUI assigné sur " + gameObject.name);
    }

    public void Toggle()
    {
        if (targetUI == null)
        {
            Debug.LogWarning("Target UI non assigné !");
            return;
        }

        bool isActive = targetUI.activeSelf;
        targetUI.SetActive(!isActive);
        Debug.Log("Toggle: " + targetUI.name + " → " + !isActive);
    }
}
