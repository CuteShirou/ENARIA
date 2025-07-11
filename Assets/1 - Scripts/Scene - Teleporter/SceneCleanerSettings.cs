
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Tools/Scene Cleaner Settings")]
public class SceneCleanerSettings : ScriptableObject
{
#if UNITY_EDITOR
    public List<SceneAsset> excludedScenes = new();
#endif
}
