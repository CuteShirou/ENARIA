using UnityEngine;

public class SkillTreeBootstrap : MonoBehaviour
{
    void Start()
    {
        var all = Resources.FindObjectsOfTypeAll<SkillTreeManager>();
        SkillTreeManager found = null;

        if (all != null && all.Length > 0)
        {
            foreach (var s in all)
            {
                if (s.gameObject.scene.isLoaded)
                {
                    found = s;
                    break;
                }
            }
            if (found == null) found = all[0];
        }

        if (found != null)
        {
            found.EnsureSyncedToCombatBook();
            Debug.Log($"[Bootstrap] Sync executed on '{found.name}'.");
        }
        else
        {
            Debug.LogWarning("[Bootstrap] Aucun SkillTreeManager trouvé.");
        }
    }
}
