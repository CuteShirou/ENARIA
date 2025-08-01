using UnityEngine;
using Mirror;

//------------------------------------------------------------
// État logique de la tuile en combat (synchronisé sur tous les clients)
public enum TileOccupation
{
    Free,     // La case est libre
    Occupied, // La case est occupée par une entité
    Blocked   // La case est bloquée (obstacle, inaccessible)
}

//------------------------------------------------------------
public class Info_NetworkTile : NetworkBehaviour
{
    [Header("État réseau de la case")]
    [SyncVar(hook = nameof(OnOccupationChanged))]
    public TileOccupation occupationState = TileOccupation.Free;

    //------------------------------------------------------------
    // Appelé automatiquement par Mirror quand occupationState change
    private void OnOccupationChanged(TileOccupation oldValue, TileOccupation newValue)
    {
        Debug.Log($"[Tile Info] Occupation changée : {oldValue} → {newValue}");
        // Ici, on pourrait déclencher un effet visuel plus tard
    }

    //------------------------------------------------------------
    // Marquer la case comme occupée (appel côté serveur)
    [Server]
    public void SetOccupied()
    {
        occupationState = TileOccupation.Occupied;
    }

    //------------------------------------------------------------
    // Libérer la case (appel côté serveur)
    [Server]
    public void SetFree()
    {
        occupationState = TileOccupation.Free;
    }

    //------------------------------------------------------------
    // Bloquer la case (appel côté serveur)
    [Server]
    public void SetBlocked()
    {
        occupationState = TileOccupation.Blocked;
    }

    //------------------------------------------------------------
    // Vérifier si la case est libre
    public bool IsFree()
    {
        return occupationState == TileOccupation.Free;
    }

    //------------------------------------------------------------
    // Vérifier si la case est occupée
    public bool IsOccupied()
    {
        return occupationState == TileOccupation.Occupied;
    }

    //------------------------------------------------------------
    // Vérifier si la case est bloquée
    public bool IsBlocked()
    {
        return occupationState == TileOccupation.Blocked;
    }
}
