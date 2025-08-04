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
public class Setup_NetworkTile : NetworkBehaviour
{
    [Header("Références")]
    public Renderer tileRenderer;

    [Header("Matériaux assignés")]
    public Material matNone;
    public Material matTeamGreen;
    public Material matTeamRed;
    public Material matObstacle;
    public Material matFighterActif;
    public Material matCursorIndicator;

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
    private void OnTileNameChanged(string _, string newName)
    {
        name = newName;
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
                tileRenderer.material = matNone;
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
            transform.SetParent(parent.transform, true);
            Debug.Log($"[NetworkTile] Parent assigné via NetId → {parent.name}");
        }
        else
        {
            Debug.LogWarning($"[NetworkTile] Impossible de retrouver parentNetId: {parentNetId} côté client.");
        }

        // Nom de tuile mis à jour via hook
        name = syncedName;

        UpdateMaterial();
    }

    //------------------------------------------------------------
    public void SetTileCoordinates(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

    //------------------------------------------------------------
    private void OnMouseDown()
    {
        if (!isClient || NetworkClient.connection == null) return;

        var localPlayerObj = NetworkClient.connection.identity;
        if (localPlayerObj == null)
        {
            Debug.LogWarning("[Tile] Aucun joueur local trouvé.");
            return;
        }

        Player_ControllerPhasePreparation playerController = localPlayerObj.GetComponent<Player_ControllerPhasePreparation>();
        if (playerController == null)
        {
            Debug.LogWarning("[Tile] Le joueur local n'a pas de script Player_ControllerPhasePreparation.");
            return;
        }

        playerController.RequestTileClick(tileX, tileY);
    }
}
