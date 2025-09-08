using UnityEngine;

//------------------------------------------------------------
// Type de la case (lié à l’équipe ou l’environnement)
//------------------------------------------------------------
public enum Tile_State
{
    None,
    TeamGreen,
    TeamRed,
    Obstacle
}

public class SetupTile : MonoBehaviour
{
    [Header("Position de la tuile dans la grille")]
    public int tileX;
    public int tileY;

    [Header("État de la tuile")]
    [SerializeField] private Tile_State _currentState = Tile_State.None;
    public Tile_State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            Tile_State old = _currentState;
            _currentState = value;
            OnTileStateChanged(old, _currentState);
        }
    }

    [Header("Statut du combattant actif")]
    [SerializeField] private bool _isFighterActif = false;
    public bool isFighterActif
    {
        get => _isFighterActif;
        set
        {
            if (_isFighterActif == value) return;
            bool old = _isFighterActif;
            _isFighterActif = value;
            OnFighterActifChanged(old, _isFighterActif);
        }
    }

    [Header("Nom synchronisé (utilisé pour debug/affichage)")]
    [SerializeField] private string _syncedName;
    public string syncedName
    {
        get => _syncedName;
        set
        {
            if (_syncedName == value) return;
            _syncedName = value;
            OnTileNameChanged(_syncedName);
        }
    }

    //------------------------------------------------------------
    // Utilitaire : définit les coordonnées de la tuile
    public void SetTileCoordinates(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

    //------------------------------------------------------------
    // "Hooks" locaux (équivalents des hooks réseau Mirror)
    private void OnTileStateChanged(Tile_State oldState, Tile_State newState)
    {
        Debug.Log($"[Tile][LOCAL] État changé : {oldState} → {newState}");
        // Le script visuel client s’occupera de la couleur/matériau
    }

    private void OnFighterActifChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Tile][LOCAL] isFighterActif modifié : {newValue}");
        // Le script visuel client s’occupera de l’indication
    }

    private void OnTileNameChanged(string newName)
    {
        name = newName; // renomme le GameObject directement
    }
}
