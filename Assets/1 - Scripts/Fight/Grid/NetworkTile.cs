using UnityEngine;
using Mirror;

//------------------------------------------------------------
public enum TileState
{
    None,
    TeamGreen,
    TeamRed,
    Obstacle
}

//------------------------------------------------------------
public class NetworkTile : NetworkBehaviour
{
    [Header("Références")]
    public Renderer tileRenderer;

    [Header("Matériaux assignés")]
    public Material matTeamGreen;
    public Material matTeamRed;
    public Material matObstacle;
    public Material matFighterActif;
    public Material matCursorIndicator;

    // État principal de la tuile (synchronisé sur tous les clients)
    [SyncVar(hook = nameof(OnTileStateChanged))]
    public TileState currentState = TileState.None;

    // Détermine si c'est la case du joueur actif
    [SyncVar(hook = nameof(OnFighterActifChanged))]
    public bool isFighterActif = false;

    // ID réseau du parent (TileGridRoot) défini côté serveur
    [SyncVar]
    public uint parentNetId;

    // État local du survol souris (non synchronisé)
    private bool isMouseOver = false;

    //------------------------------------------------------------
    private void Start()
    {
        UpdateMaterial(); // Mise à jour visuelle au lancement
    }

    //------------------------------------------------------------
    private void OnTileStateChanged(TileState oldState, TileState newState)
    {
        Debug.Log($"[Tile] Changement d'état réseau : {oldState} → {newState}");
        UpdateMaterial();
    }

    //------------------------------------------------------------
    private void OnFighterActifChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Tile] isFighterActif modifié : {newValue}");
        UpdateMaterial();
    }

    //------------------------------------------------------------
    private void UpdateMaterial()
    {
        if (tileRenderer == null) return;

        // Cas spécial : joueur actif (prioritaire)
        if (isFighterActif && matFighterActif != null)
        {
            tileRenderer.material = matFighterActif;
            return;
        }

        // Matériau selon état réseau
        switch (currentState)
        {
            case TileState.TeamGreen:
                tileRenderer.material = matTeamGreen;
                break;
            case TileState.TeamRed:
                tileRenderer.material = matTeamRed;
                break;
            case TileState.Obstacle:
                tileRenderer.material = matObstacle;
                break;
            default:
                tileRenderer.material = null;
                break;
        }
    }

    //------------------------------------------------------------
    private void OnMouseEnter()
    {
        isMouseOver = true;
        if (!isFighterActif && matCursorIndicator != null)
            tileRenderer.material = matCursorIndicator;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        UpdateMaterial(); // Revenir au bon état
    }

    //------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Reparentage côté client via parentNetId
        if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parent))
        {
            transform.SetParent(parent.transform, true); // true : conserve la position
            Debug.Log($"[NetworkTile] Parent assigné via NetId → {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[NetworkTile] Impossible de retrouver parentNetId: {parentNetId} côté client.");
        }

        UpdateMaterial(); // Applique les bonnes couleurs
    }
}
