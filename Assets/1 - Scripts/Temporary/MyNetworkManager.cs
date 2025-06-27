using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            Debug.LogWarning("⛔ Connexion déjà liée à un joueur !");
            return;
        }

        Transform start = GetStartPosition();
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        Debug.Log("✅ AddPlayerForConnection : " + conn.connectionId);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        Debug.Log("🟢 Client connecté, envoie AddPlayer");
        NetworkClient.Send(new AddPlayerMessage()); // ⬅️ force le client à demander un joueur
    }
}