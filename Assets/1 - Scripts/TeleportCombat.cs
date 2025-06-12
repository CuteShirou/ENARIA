
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TeleporterCombat : MonoBehaviour
{


    [Header("Transform cible dans la scène à charger (destination du joueur)")]
    [SerializeField] private Transform destinationTransform;

    [SerializeField] private string playerTag = "Player";


    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<ThirdPersonController>()._isInCombat = true;
        // Déplacement du joueur si une destination est spécifiée
        if (destinationTransform != null)
        {
            other.transform.position = destinationTransform.position;
            other.transform.rotation = destinationTransform.rotation;
        }
    }
}
