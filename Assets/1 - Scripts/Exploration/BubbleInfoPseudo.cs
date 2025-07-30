using UnityEngine;
using TMPro;

//----------------------------------------------------------
public class BubbleInfoPseudo : MonoBehaviour
{
    [Header("Référence au texte 3D")]
    public TextMeshPro textMesh;

    [Header("Afficher seulement au survol souris")]
    public bool hideOutsideHover = true;

    private Transform targetCamera;

    private void Start()
    {
        targetCamera = Camera.main?.transform;

        // Cache le texte si option activée
        if (hideOutsideHover && textMesh != null)
            textMesh.gameObject.SetActive(false);
    }

    // Appelé depuis SetupPlayer
    public void SetPseudo(string playerName)
    {
        if (textMesh != null)
        {
            textMesh.text = playerName;
            Debug.Log("[BubbleInfoPseudo] Pseudo assigné : " + playerName);
        }
    }

    private void LateUpdate()
    {
        if (targetCamera != null)
            transform.forward = targetCamera.forward;
    }

    // Affiche le pseudo si souris dessus
    private void OnMouseEnter()
    {
        if (hideOutsideHover && textMesh != null)
            textMesh.gameObject.SetActive(true);
    }

    // Cache le pseudo si la souris quitte
    private void OnMouseExit()
    {
        if (hideOutsideHover && textMesh != null)
            textMesh.gameObject.SetActive(false);
    }
}
