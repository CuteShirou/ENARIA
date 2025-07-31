using UnityEngine;
using Mirror;

//-------------------------------------------------------------
public class Player_ControllerPhasePreparation : NetworkBehaviour
{
    //-------------------------------------------------------------
    // Appelée par la tuile quand elle est cliquée (client local uniquement)
    public void RequestTileClick(int x, int y)
    {
        // Vérifie qu'on est bien le joueur local
        if (!isLocalPlayer) return;

        // Envoie au serveur la demande de clic
        CmdClickTile(x, y);
    }

    //-------------------------------------------------------------
    // Commande envoyée au serveur quand le joueur clique sur une tuile
    [Command]
    private void CmdClickTile(int x, int y)
    {
        // Le serveur affiche un message de debug
        Debug.Log($"[SERVER] Le joueur {gameObject.name} a cliqué sur la case ({x}, {y})");

        // Envoie un message à tous les clients
        RpcAnnounceTileClick(gameObject.name, x, y);
    }

    //-------------------------------------------------------------
    // Reçu par tous les clients pour afficher le clic
    [ClientRpc]
    private void RpcAnnounceTileClick(string playerName, int x, int y)
    {
        Debug.Log($"[CLIENT] Le joueur {playerName} a cliqué sur la case ({x}, {y})");
    }
}
