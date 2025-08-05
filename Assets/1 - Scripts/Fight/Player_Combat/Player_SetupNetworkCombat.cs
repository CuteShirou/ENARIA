using UnityEngine;
using Mirror;

//------------------------------------------------------------
// Gère le parentage et la position initiale d'un joueur en combat
public class Player_SetupNetworkCombat : NetworkBehaviour
{
    [Header("ID du parent défini côté serveur (TeamVerte)")]
    [SyncVar(hook = nameof(OnParentChanged))]
    public uint parentNetId;

    [Header("Position initiale à synchroniser")]
    [SyncVar(hook = nameof(OnPositionUpdated))]
    private Vector3 syncedPosition;

    [Header("Référence au CombatManager de l'arène du Joueur")]
    [SyncVar] public NetworkIdentity combatManagerIdentity;

    //------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Applique immédiatement la position synchronisée
        transform.position = syncedPosition;
        Debug.Log($"[Player_SetupNetworkCombat] Position initiale reçue : {syncedPosition}");

        // Applique le parent via le hook
        OnParentChanged(0, parentNetId);
    }

    //------------------------------------------------------------
    // Appelée par le serveur lors du placement
    [Server]
    public void SetInitialPosition(Vector3 position)
    {
        syncedPosition = position;
    }

    //------------------------------------------------------------
    // Hook appelé côté client quand la position change
    private void OnPositionUpdated(Vector3 oldPos, Vector3 newPos)
    {
        transform.position = newPos;
        Debug.Log($"[Player_SetupNetworkCombat] Position mise à jour côté client : {newPos}");
    }

    //------------------------------------------------------------
    // Hook appelé côté client quand le parent change (reparentage hiérarchique)
    private void OnParentChanged(uint oldId, uint newId)
    {
        if (NetworkClient.spawned.TryGetValue(newId, out NetworkIdentity parent))
        {
            transform.SetParent(parent.transform, true); // conserve la position monde
            Debug.Log($"[Player_SetupNetworkCombat] Reparentage dynamique → {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[Player_SetupNetworkCombat] Impossible de trouver le parent NetId: {newId}");
        }
    }
}
