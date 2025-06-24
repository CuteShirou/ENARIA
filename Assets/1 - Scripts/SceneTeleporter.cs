
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using StarterAssets;
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
            TPC._isClickMoving = false;
            TPC._clickTarget = Vector3.zero;

            if (TPC._hasAnimator)
            {
                TPC._animator.SetFloat(TPC._animIDSpeed, 0f);
                TPC._animator.SetFloat(TPC._animIDMotionSpeed, 0f);
            }
        }
    }

    private IEnumerator SwitchSceneAdditive(GameObject player)
    {
        string sceneToUnload = gameObject.scene.name;

        Debug.Log("[SceneTeleporter] Active scene BEFORE load: " + SceneManager.GetActiveScene().name);
        Debug.Log("[SceneTeleporter] Loading scene: " + sceneName);
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            Debug.Log("[SceneTeleporter] New scene set active: " + newScene.name);
        }
        else
        {
            Debug.LogError("[SceneTeleporter] Failed to load new scene: " + sceneName);
        }

        yield return null;

        if (destinationTransform != null)
        {
            player.transform.position = destinationTransform.position;
            player.transform.rotation = destinationTransform.rotation;
        }

        yield return null;
        SetCameraParentByName(cameraParentTargetName);

        yield return null;
        Debug.Log("[SceneTeleporter] Unloading scene: " + sceneToUnload);
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
        while (!unloadOp.isDone)
            yield return null;
    }

    private void SetCameraParentByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            Debug.LogWarning("[SceneTeleporter] Camera parent target name is empty.");
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[SceneTeleporter] MainCamera not found.");
            return;
        }

        Debug.Log("[SceneTeleporter] MainCamera found: " + mainCam.name);

        ParentConstraint constraint = mainCam.GetComponent<ParentConstraint>();
        if (constraint == null)
        {
            Debug.LogWarning("[SceneTeleporter] No ParentConstraint found on MainCamera. Skipping camera reassignment.");
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
        {
            Debug.Log("[SceneTeleporter] Camera parent switched to: " + targetName);
        }
        else
        {
            Debug.LogWarning("[SceneTeleporter] Target camera parent " + targetName + " not found among constraint sources.");
        }
    }
}
