using UnityEngine;

//------------------------------------------------------------
// État logique de la tuile en combat (LOCAL, sans réseau)
//------------------------------------------------------------
public enum TileOccupation
{
    Free,     // La case est libre
    Occupied, // La case est occupée par une entité
    Blocked   // La case est bloquée (obstacle, inaccessible)
}

//------------------------------------------------------------
public class InfoTile : MonoBehaviour
{
    [Header("État logique de la case (local)")]
    [SerializeField] private TileOccupation _occupationState = TileOccupation.Free;

    public TileOccupation occupationState
    {
        get => _occupationState;
        set
        {
            if (_occupationState == value) return;
            TileOccupation old = _occupationState;
            _occupationState = value;
            OnOccupationChanged(old, _occupationState);
        }
    }

    //------------------------------------------------------------
    // Hooks locaux
    private void OnOccupationChanged(TileOccupation oldValue, TileOccupation newValue)
    {
        Debug.Log($"[InfoTile][LOCAL] Occupation changée : {oldValue} → {newValue}");
    }

    //------------------------------------------------------------
    // Méthodes de mise à jour
    public void SetOccupied() => occupationState = TileOccupation.Occupied;
    public void SetFree() => occupationState = TileOccupation.Free;
    public void SetBlocked() => occupationState = TileOccupation.Blocked;

    //------------------------------------------------------------
    // Helpers de lecture
    public bool IsFree() => occupationState == TileOccupation.Free;
    public bool IsOccupied() => occupationState == TileOccupation.Occupied;
    public bool IsBlocked() => occupationState == TileOccupation.Blocked;
}
