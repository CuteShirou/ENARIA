
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using Mirror;
using System.Collections;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Nom EXACT de la scène à charger (sans .unity)")]
    [SerializeField] private string sceneName;

    [Header("Transform cible dans la scène à charger (destination du joueur)")]
    [SerializeField] private Transform destinationTransform;

    [Header("Nom du parent de caméra à activer (ParentConstraint)")]
    [SerializeField] private string cameraParentTargetName;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !string.IsNullOrEmpty(sceneName))
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
        string sceneToUnload = gameObject.scene.name;

        Debug.Log("[SceneTeleporter] Active scene BEFORE load: " + SceneManager.GetActiveScene().name);
        Debug.Log("[SceneTeleporter] Loading scene: " + sceneName);

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone)
                yield return null;
        }
        else
        {
            Debug.Log($"[SceneTeleporter] La scène '{sceneName}' est déjà chargée localement.");
        }

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            SceneManager.MoveGameObjectToScene(player, newScene);
            Debug.Log("[SceneTeleporter] New scene set active: " + newScene.name);
        }
        else
        {
            Debug.LogError("[SceneTeleporter] Failed to load new scene: " + sceneName);
        }

        yield return null;

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

        yield return null;
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                Debug.Log("[SceneTeleporter] Déchargement local de la scène : " + sceneToUnload);
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                while (!unloadOp.isDone)
                    yield return null;
            }
        }
        else
        {
            Debug.Log($"[SceneTeleporter] La scène '{sceneName}' est déjà chargée localement.");
        }
    }

    private void SetCameraParentByName(GameObject player, string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            Debug.LogWarning("[SceneTeleporter] Camera parent target name is empty.");
            return;
        }

        Camera playerCam = player.GetComponentInChildren<Camera>(true);
        if (playerCam == null)
        {
            Debug.LogError("[SceneTeleporter] Local player camera not found.");
            return;
        }

        ParentConstraint constraint = playerCam.GetComponent<ParentConstraint>();
        if (constraint == null)
        {
            Debug.LogWarning("[SceneTeleporter] No ParentConstraint found on local camera.");
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

        if (found)
            Debug.Log($"🎥 [SceneTeleporter] Local camera parent switched to: {targetName}");
        else
            Debug.LogWarning($"❌ [SceneTeleporter] Target camera source '{targetName}' not found.");
    }
}
