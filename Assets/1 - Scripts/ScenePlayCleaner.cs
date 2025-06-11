
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public static class ScenePlayCleaner
{
    private static bool alreadyCleaned = false;
    private const string SETTINGS_PATH = "Assets/SceneCleanerSettings.asset";

    private static List<string> scenesToRestore = new();

    static ScenePlayCleaner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && !alreadyCleaned)
        {
            var settings = AssetDatabase.LoadAssetAtPath<SceneCleanerSettings>(SETTINGS_PATH);

            if (settings == null)
            {
                Debug.LogWarning($"SceneCleanerSettings.asset not found at path: {SETTINGS_PATH}");
                return;
            }

            // Sauvegarder les scènes ouvertes pour restauration
            scenesToRestore.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.IsValid() && !string.IsNullOrEmpty(s.path))
                    scenesToRestore.Add(s.path);
            }

            // Annuler temporairement le Play Mode
            EditorApplication.isPlaying = false;

            HashSet<string> exclusions = new();
            foreach (var sceneAsset in settings.excludedScenes)
            {
                if (sceneAsset != null)
                    exclusions.Add(sceneAsset.name);
            }

            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.name != activeScene.name && !exclusions.Contains(s.name))
                {
                    EditorSceneManager.CloseScene(s, false);
                    Debug.Log($"ScenePlayCleaner: Fermeture scène -> {s.name}");
                }
            }

            alreadyCleaned = true;
            EditorApplication.delayCall += () =>
            {
                EditorApplication.isPlaying = true;
                alreadyCleaned = false;
            };
        }
        else if (state == PlayModeStateChange.EnteredEditMode && scenesToRestore.Count > 0)
        {
            EditorSceneManager.OpenScene(scenesToRestore[0], OpenSceneMode.Single);
            for (int i = 1; i < scenesToRestore.Count; i++)
            {
                EditorSceneManager.OpenScene(scenesToRestore[i], OpenSceneMode.Additive);
            }

            Debug.Log("ScenePlayCleaner: Scènes restaurées après le Play Mode.");
            scenesToRestore.Clear();
        }
    }
}
