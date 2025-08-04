using UnityEngine;
using Mirror;

//------------------------------------------------------------
// Type de la case (lié à l’équipe ou l’environnement)
//------------------------------------------------------------
public enum TileState
{
    None,
    TeamGreen,
    TeamRed,
    Obstacle
}

//------------------------------------------------------------
// Composant réseau : état de base de la case, synchronisé
//------------------------------------------------------------
public class Setup_NetworkTile : NetworkBehaviour
{
    [Header("Position de la tuile dans la grille")]
    [SyncVar] public int tileX;
    [SyncVar] public int tileY;

    // État principal de la tuile (synchronisé sur tous les clients)
    [SyncVar(hook = nameof(OnTileStateChanged))]
    public TileState currentState = TileState.None;

    // Détermine si c'est la case du joueur actif
    [SyncVar(hook = nameof(OnFighterActifChanged))]
    public bool isFighterActif = false;

    // ID réseau du parent (TileGridRoot) défini côté serveur
    [SyncVar] public uint parentNetId;

    // Nom de la tuile synchronisé (ex : Case_3_5)
    [SyncVar(hook = nameof(OnTileNameChanged))]
    public string syncedName;

    //------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Reparentage côté client via parentNetId
        if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parent))
        {
            transform.SetParent(parent.transform, true); // conserve la position monde
            Debug.Log($"[NetworkTile] Parent assigné via NetId → {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[NetworkTile] Impossible de retrouver parentNetId: {parentNetId} côté client.");
        }

        // Nom de tuile mis à jour via hook
        name = syncedName;
    }

    //------------------------------------------------------------
    private void OnTileStateChanged(TileState oldState, TileState newState)
    {
        Debug.Log($"[Tile] Changement d'état réseau : {oldState} → {newState}");
        // Le script client visuel s'occupera du matériau
    }

    private void OnFighterActifChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Tile] isFighterActif modifié : {newValue}");
        // Le script client visuel s'occupera du matériau
    }

    private void OnTileNameChanged(string _, string newName)
    {
        name = newName;
    }

    //------------------------------------------------------------
    public void SetTileCoordinates(int x, int y)
    {
        tileX = x;
        tileY = y;
    }
}
