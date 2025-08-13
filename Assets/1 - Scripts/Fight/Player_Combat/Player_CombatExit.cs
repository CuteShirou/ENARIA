using System.Text;
using UnityEngine;
using Mirror;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class Player_CombatExit : NetworkBehaviour
{
    private Entity_Info entityInfo;                   // Données sauvegardées (pos/cam)
    private Entity_StatistiqueCombat stats;           // État réseau (isFight/isReady)

    [Header("Fallback si parent d'exploration non fourni")]
    [SerializeField] private string explorationParentName = "= PLAYER LIST"; // Nom par défaut du parent Exploration

    //----------------------------------------------------------
    // Awake : récupère les composants utiles
    //----------------------------------------------------------
    private void Awake()
    {
        // FR : Récupération des composants nécessaires sur le Player
        entityInfo = GetComponent<Entity_Info>();
        stats = GetComponent<Entity_StatistiqueCombat>();
    }

    //==========================================================
    //  A) SYNC DU PARENT AU SPAWN (demandée par MyNetworkManager)
    //==========================================================

    //----------------------------------------------------------
    // ServerSyncParentAtSpawn : appelé par MyNetworkManager APRÈS AddPlayerForConnection
    // - Confirme le parent côté serveur (si null → recherche globale)
    // - Envoie aux clients (scene + chemin hiérarchique) pour reparent local
    //----------------------------------------------------------
    [Server]
    public void ServerSyncParentAtSpawn(Transform explicitParent)
    {
        Transform targetParent = explicitParent != null ? explicitParent : GetExplorationParentServer();

        if (targetParent == null)
        {
            Debug.LogWarning("[Player_CombatExit] ServerSyncParentAtSpawn : parent introuvable, sync ignorée.");
            return;
        }

        // FR : S'assure côté serveur qu'on est bien parenté (au cas où)
        transform.SetParent(targetParent, true);

        string parentSceneName = targetParent.gameObject.scene.name;
        string parentHierarchyPath = BuildHierarchyPath(targetParent);

        // FR : Pousse l'info à tous les clients (reparent local, cross-scene autorisé)
        RpcClientSetParentAtSpawn(parentSceneName, parentHierarchyPath);
    }

    //----------------------------------------------------------
    // RpcClientSetParentAtSpawn : reparent chez tous les clients au spawn
    //----------------------------------------------------------
    [ClientRpc]
    private void RpcClientSetParentAtSpawn(string sceneName, string hierarchyPath)
    {
        Transform parent = FindBySceneAndPath(sceneName, hierarchyPath);
        if (parent == null)
        {
            // Fallback si le chemin complet ne fonctionne pas
            string[] parts = hierarchyPath.Split('/');
            string last = parts.Length > 0 ? parts[^1] : explorationParentName;
            parent = FindParentGlobal(last);
        }

        if (parent != null)
        {
            transform.SetParent(parent, true);
        }
        else
        {
            Debug.LogWarning($"[Player_CombatExit] Client (spawn) : parent introuvable (scene='{sceneName}', path='{hierarchyPath}').");
        }
    }

    //==========================================================
    //  B) ABANDON (déjà implémenté précédemment)
    //==========================================================

    //----------------------------------------------------------
    // CmdRequestAbandon : appelée par le client local, exécutée sur le serveur
    // - Libère la tuile, retire des listes
    // - isFight=false, téléport à savePosEntity
    // - Reparent exploration (server) + propagation clients
    // - Restaure caméra (TargetRpc propriétaire)
    //----------------------------------------------------------
    [Command]
    public void CmdRequestAbandon()
    {
        if (!isServer) return;

        if (stats == null)
        {
            Debug.LogWarning("[Player_CombatExit] Stats manquantes, abandon ignoré.");
            return;
        }

        // 1) Trouver une arène où ce joueur serait listé et nettoyer
        Combat_PhaseManager manager = FindManagerForThisPlayer();
        if (manager != null)
        {
            if (manager.tileGrid != null)
                manager.tileGrid.UnregisterEntity(gameObject); // libère la tuile

            if (manager.phaseEnter != null)
            {
                manager.phaseEnter.greenTeam.Remove(gameObject);
                manager.phaseEnter.AllFighters.Remove(gameObject);
            }
        }

        // 2) Flags réseau
        stats.isReady = false;
        stats.isFight = false;

        // 3) Téléportation serveur vers la position sauvegardée
        Vector3 targetPos = entityInfo != null ? entityInfo.savePosEntity : transform.position;
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = targetPos;
        if (cc != null) cc.enabled = true;

        // 4) Reparent exploration (serveur) + sync clients
        Transform targetParent = GetExplorationParentServer();
        if (targetParent != null)
        {
            transform.SetParent(targetParent, true);

            string parentSceneName = targetParent.gameObject.scene.name;
            string parentHierarchyPath = BuildHierarchyPath(targetParent);

            RpcClientTeleportAndReparent(targetPos, parentSceneName, parentHierarchyPath);
        }
        else
        {
            // Même si le parent n'est pas trouvé, pousser au moins la position aux clients
            RpcClientTeleportAndReparent(targetPos, "", "");
            Debug.LogWarning("[Player_CombatExit] Parent exploration (server) introuvable à l'abandon.");
        }

        // 5) Caméra propriétaire (ParentConstraint, même logique que Teleporter.cs)
        string camParentTargetName = (entityInfo != null) ? entityInfo.saveCamEntity : "";
        TargetClientRestoreCamera(connectionToClient, camParentTargetName);

        // 6) Garde-fou
        if (manager != null)
            manager.TryStopCombatIfNoPlayers();
    }

    //----------------------------------------------------------
    // RpcClientTeleportAndReparent : applique pos + parent (abandon)
    //----------------------------------------------------------
    [ClientRpc]
    private void RpcClientTeleportAndReparent(Vector3 pos, string sceneName, string hierarchyPath)
    {
        // FR : Téléportation locale
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = pos;
        if (cc != null) cc.enabled = true;

        // FR : Reparent local
        Transform parent = FindBySceneAndPath(sceneName, hierarchyPath);
        if (parent == null && !string.IsNullOrEmpty(hierarchyPath))
        {
            string[] parts = hierarchyPath.Split('/');
            string last = parts.Length > 0 ? parts[^1] : explorationParentName;
            parent = FindParentGlobal(last);
        }

        if (parent != null)
        {
            transform.SetParent(parent, true);
        }
        else if (!string.IsNullOrEmpty(hierarchyPath))
        {
            Debug.LogWarning($"[Player_CombatExit] Client (abandon) : parent introuvable (scene='{sceneName}', path='{hierarchyPath}').");
        }
    }

    //==========================================================
    //  Utils serveur/clients partagés (chemins, recherche, etc.)
    //==========================================================

    [Server]
    private Transform GetExplorationParentServer()
    {
        var nm = NetworkManager.singleton as MyNetworkManager;
        if (nm != null && nm.PlayerParent != null)
            return nm.PlayerParent;

        return FindParentGlobal(explorationParentName);
    }

    [Server]
    private Combat_PhaseManager FindManagerForThisPlayer()
    {
        var managers = Object.FindObjectsByType<Combat_PhaseManager>(FindObjectsSortMode.None);
        foreach (var m in managers)
        {
            if (m != null && m.phaseEnter != null)
            {
                if (m.phaseEnter.greenTeam.Contains(gameObject) || m.phaseEnter.AllFighters.Contains(gameObject))
                    return m;
            }
        }
        return null;
    }

    private string BuildHierarchyPath(Transform t)
    {
        var sb = new StringBuilder(128);
        while (t != null)
        {
            sb.Insert(0, "/" + t.name);
            t = t.parent;
        }
        return sb.ToString();
    }

    private Transform FindBySceneAndPath(string sceneName, string hierarchyPath)
    {
        if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(hierarchyPath)) return null;

        Scene s = SceneManager.GetSceneByName(sceneName);
        if (!s.IsValid() || !s.isLoaded) return null;

        string[] parts = hierarchyPath.Split('/');
        if (parts.Length < 2) return null;

        string rootName = parts[1];
        GameObject rootGO = null;
        foreach (var root in s.GetRootGameObjects())
        {
            if (root.name == rootName)
            {
                rootGO = root;
                break;
            }
        }
        if (rootGO == null) return null;

        Transform current = rootGO.transform;
        for (int i = 2; i < parts.Length; i++)
        {
            string wanted = parts[i];
            if (string.IsNullOrEmpty(wanted)) continue;

            bool found = false;
            for (int c = 0; c < current.childCount; c++)
            {
                var child = current.GetChild(c);
                if (child.name == wanted)
                {
                    current = child;
                    found = true;
                    break;
                }
            }
            if (!found) return null;
        }

        return current;
    }

    private Transform FindParentGlobal(string wantedName)
    {
        if (string.IsNullOrEmpty(wantedName)) return null;

        int count = SceneManager.sceneCount;
        for (int i = 0; i < count; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;

            var roots = s.GetRootGameObjects();
            foreach (var root in roots)
            {
                var found = FindChildRecursiveByName(root.transform, wantedName);
                if (found != null) return found;
            }
        }
        return null;
    }

    private Transform FindChildRecursiveByName(Transform parent, string wantedName)
    {
        if (parent.name == wantedName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var found = FindChildRecursiveByName(child, wantedName);
            if (found != null) return found;
        }
        return null;
    }

    //==========================================================
    //  Caméra (abandon) – déjà gérée par TargetClientRestoreCamera()
    //==========================================================

    [TargetRpc]
    private void TargetClientRestoreCamera(NetworkConnectionToClient conn, string camName)
    {
        if (string.IsNullOrEmpty(camName)) return;

        Camera playerCam = GetComponentInChildren<Camera>(true);
        if (playerCam == null) return;

        ParentConstraint constraint = playerCam.GetComponent<ParentConstraint>();
        if (constraint == null) return;

        for (int i = 0; i < constraint.sourceCount; i++)
        {
            ConstraintSource src = constraint.GetSource(i);
            bool match = (src.sourceTransform != null && src.sourceTransform.name == camName);
            src.weight = match ? 1f : 0f;
            constraint.SetSource(i, src);
        }
    }
}
