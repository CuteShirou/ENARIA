using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UI/EndFight Line UI")]
public class EndFight_LineUI : MonoBehaviour
{
    [Header("Drop Container")]
    [SerializeField] private Transform dropContent; // Drag: Scroll_Drop_Ressource/Viewport/Content

    // SetDrops : instancie les prefabs reçus dans le content
    public void SetDrops(List<GameObject> dropPrefabs)
    {
        if (!dropContent || dropPrefabs == null) return;

        // Vide le content (ré-usage du prefab possible)
        for (int i = dropContent.childCount - 1; i >= 0; i--)
        {
            var c = dropContent.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(c.gameObject);
            else
#endif
                Object.Destroy(c.gameObject);
        }

        // Instancie chaque prefab (déjà configuré avec Drop_Loot/Item/etc.)
        foreach (var p in dropPrefabs)
        {
            if (!p) continue;
            var go = Instantiate(p, dropContent, false);
            go.name = p.name;
        }
    }
}
