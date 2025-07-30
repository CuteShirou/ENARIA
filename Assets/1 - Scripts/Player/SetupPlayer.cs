using UnityEngine;
using Mirror;

//----------------------------------------------------------
public class SetupPlayer : NetworkBehaviour
{
    // Appelé sur tous (clients + serveur)
    void Start()
    {
        // Le serveur (dans l'éditeur) a besoin de renommer directement ici
        if (isServer)
        {
            string newName = "Player " + netId;
            gameObject.name = newName;
            Debug.Log("[SetupPlayer][Server] Nom forcé dans Start() : " + newName);
        }

        // Pour les clients aussi, on force un renommage local
        if (isClient)
        {
            string newName = "Player " + netId;
            gameObject.name = newName;
            Debug.Log("[SetupPlayer][Client] Nom local : " + newName);
        }
    }
}
