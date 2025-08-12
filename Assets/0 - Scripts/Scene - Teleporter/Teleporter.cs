
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using Mirror;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("Transform cible dans la scène à charger (destination du joueur)")]
    [SerializeField] private Transform destinationTransform;

    [Header("Nom du parent de caméra à activer (ParentConstraint)")]
    [SerializeField] private string cameraParentTargetName;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            DontDestroyOnLoad(other.transform.root.gameObject);
            StartCoroutine(SwitchSceneAdditive(other.gameObject));

            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
        }
    }

    private IEnumerator SwitchSceneAdditive(GameObject player)
    {
        if (destinationTransform != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = destinationTransform.position;
            player.transform.rotation = destinationTransform.rotation;

            if (cc != null) cc.enabled = true;

            Debug.Log($"✅ Joueur déplacé à {player.transform.position} dans scène {player.scene.name}");
        }

        yield return null;
        SetCameraParentByName(player, cameraParentTargetName);
    }

    private void SetCameraParentByName(GameObject player, string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            return;
        }

        Camera playerCam = player.GetComponentInChildren<Camera>(true);
        if (playerCam == null)
        {
            return;
        }

        ParentConstraint constraint = playerCam.GetComponent<ParentConstraint>();
        if (constraint == null)
        {
            return;
        }

        bool found = false;
        for (int i = 0; i < constraint.sourceCount; i++)
        {
            ConstraintSource src = constraint.GetSource(i);
            bool match = (src.sourceTransform != null && src.sourceTransform.name == targetName);
            src.weight = match ? 1f : 0f;
            constraint.SetSource(i, src);
            if (match) found = true;
        }
    }
}
