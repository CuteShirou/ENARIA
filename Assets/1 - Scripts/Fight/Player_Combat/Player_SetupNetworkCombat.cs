using UnityEngine;
using Mirror;

//------------------------------------------------------------
// Gère le parentage, la position initiale et l'arène du joueur
public class Player_SetupNetworkCombat : NetworkBehaviour
{
    [Header("ID du parent défini côté serveur (TeamVerte)")]
    [SyncVar]
    public uint parentNetId;

    [Header("Position initiale à synchroniser")]
    [SyncVar(hook = nameof(OnPositionUpdated))]
    private Vector3 syncedPosition;

    [Header("Arena assignée pour ce joueur")]
    [SyncVar]
    public int arenaIndex;

    //------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Reparentage : TeamVerte
        if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parent))
        {
            transform.SetParent(parent.transform, true); // garde la position actuelle
            Debug.Log($"[Player_SetupNetworkCombat] Parent trouvé : {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[Player_SetupNetworkCombat] parentNetId {parentNetId} introuvable !");
        }

        // Appliquer la position si on est le client local
        if (isLocalPlayer)
        {
            transform.position = syncedPosition;
        }
    }

    //------------------------------------------------------------
    [Server]
    public void SetInitialPosition(Vector3 position)
    {
        syncedPosition = position;
    }

    //------------------------------------------------------------
    private void OnPositionUpdated(Vector3 oldPos, Vector3 newPos)
    {
        if (isLocalPlayer)
        {
            transform.position = newPos;
            Debug.Log($"[Player_SetupNetworkCombat] Position mise à jour côté client : {newPos}");
        }
    }
}
