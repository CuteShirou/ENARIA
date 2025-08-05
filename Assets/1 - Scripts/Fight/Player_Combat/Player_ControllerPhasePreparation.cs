using UnityEngine;
using Mirror;

//-------------------------------------------------------------
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
        // Log debug serveur
        Debug.Log($"[SERVER] Le joueur {gameObject.name} a cliqué sur la case ({x}, {y})");

        // Récupère la phase de préparation via le CombatManager associé
        if (!TryGetPreparationPhase(out Phase_PreparationPlacementCombat prepa))
        {
            Debug.LogWarning("[SERVER] Impossible de trouver la phase de préparation depuis ce joueur.");
            return;
        }

        // Vérifie que la phase est bien active
        if (prepa != null && prepa.isActiveAndEnabled)
        {
            prepa.TryMoveEntityToTile(gameObject, x, y);
        }
        else
        {
            Debug.LogWarning("[SERVER] Phase_PreparationPlacementCombat introuvable ou inactive.");
        }

        // Message client visuel (inchangé)
        RpcAnnounceTileClick(gameObject.name, x, y);
    }

    //-------------------------------------------------------------
    // Reçu par tous les clients pour afficher le clic
    [ClientRpc]
    private void RpcAnnounceTileClick(string playerName, int x, int y)
    {
        Debug.Log($"[CLIENT] Le joueur {playerName} a cliqué sur la case ({x}, {y})");
    }

    //-------------------------------------------------------------
    // Essaie de retrouver la phase de préparation via Player_SetupNetworkCombat
    private bool TryGetPreparationPhase(out Phase_PreparationPlacementCombat phase)
    {
        phase = null;

        // Vérifie que ce GameObject a bien le script de setup réseau
        if (!TryGetComponent(out Player_SetupNetworkCombat setup))
        {
            Debug.LogError("[SERVER] Ce joueur n'a pas de Player_SetupNetworkCombat !");
            return false;
        }

        // Vérifie que la référence au manager a bien été synchronisée
        if (setup.combatManagerIdentity == null)
        {
            Debug.LogError("[SERVER] combatManagerIdentity non assigné !");
            return false;
        }

        // Récupère le CombatManager depuis l'identité réseau
        Combat_PhaseManager manager = setup.combatManagerIdentity.GetComponent<Combat_PhaseManager>();
        if (manager == null)
        {
            Debug.LogError("[SERVER] Impossible d'accéder au Combat_PhaseManager !");
            return false;
        }

        // Accède à la phase de préparation
        phase = manager.phasePrepa;
        return true;
    }
}
