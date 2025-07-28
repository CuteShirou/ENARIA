using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [Tooltip("Prefab de l'UI à instancier")]
    public GameObject uiPrefab;

    [Tooltip("Parent sous lequel sera instanciée l'UI")]
    public Transform uiParent;

    private GameObject instantiatedUI;

    public void Toggle()
    {
        if (instantiatedUI == null)
        {
            instantiatedUI = Instantiate(uiPrefab, uiParent ?? transform.parent);
            instantiatedUI.name = uiPrefab.name; // Pour éviter "(Clone)"
            Debug.Log("UI instanciée : " + instantiatedUI.name);
        }
        else
        {
            Destroy(instantiatedUI);
            instantiatedUI = null;
            Debug.Log("UI détruite.");
        }
    }
}
