using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    [Header("Player attached")]
    [SerializeField] private Transform playerParent; // Parent des players en exploration (ex: 'Player List')

    // FR : Accès public et en lecture seule au parent d'exploration.
    //      Utilisé par Player_CombatExit pour remettre le joueur dans le bon dossier.
    public Transform PlayerParent => playerParent;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            Debug.LogWarning("⛔ Connexion déjà liée à un joueur !");
            return;
        }

        Transform start = GetStartPosition();
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);

        // ✅ Ajouter le joueur comme enfant du parent défini (ex: 'Player List')
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
