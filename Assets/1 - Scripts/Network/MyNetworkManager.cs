using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    [Header("Player attached")]
        [SerializeField] private Transform playerParent;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            Debug.LogWarning("⛔ Connexion déjà liée à un joueur !");
            return;
        }

        Transform start = GetStartPosition();
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);

        // ✅ Ajouter le joueur comme enfant du parent défini
        if (playerParent != null)
        {
            player.transform.SetParent(playerParent);
        }
        else
        {
            Debug.LogWarning("⚠️ Aucun parent défini pour les joueurs dans le NetworkManager.");
        }

        Debug.Log("✅ AddPlayerForConnection : " + conn.connectionId);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        Debug.Log("🟢 Client connecté, envoie AddPlayer");
        NetworkClient.Send(new AddPlayerMessage());
    }
    
    void Awake()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.MoveGameObjectToScene(gameObject, currentScene);
        Debug.Log($"🔒 {gameObject.name} forcé à rester dans la scène : {currentScene.name}");
    }
}