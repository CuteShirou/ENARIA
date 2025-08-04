using UnityEngine;
using Mirror;

//------------------------------------------------------------
// Ce script remet les monstres sous TeamRed (ou autre) côté client
public class Setup_NetworkMonster : NetworkBehaviour
{
    [Header("ID du parent assigné côté serveur")]
    [SyncVar]
    public uint parentNetId;

    //------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Retrouve le parent à partir de son NetworkID
        if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parent))
        {
            transform.SetParent(parent.transform, true); // true = conserve la position
            Debug.Log($"[Monster] Parent retrouvé : {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[Monster] Impossible de retrouver le parent NetId {parentNetId}");
        }
    }
}
