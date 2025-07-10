using Mirror;
using UnityEngine;

public class DebugNetworkSpawn : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        Debug.Log("✅ OnStartLocalPlayer appelé sur : " + gameObject.name);
    }

    public override void OnStartClient()
    {
        Debug.Log("✅ OnStartClient appelé sur : " + gameObject.name);
    }

    public override void OnStartServer()
    {
        Debug.Log("✅ OnStartServer appelé sur : " + gameObject.name);
    }
}
