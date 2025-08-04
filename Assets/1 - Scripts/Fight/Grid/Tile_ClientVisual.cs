using UnityEngine;
using Mirror;

//------------------------------------------------------------
// Gère uniquement les aspects visuels et interactions souris
// Ce script doit être attaché sur le même GameObject que Setup_NetworkTile
//------------------------------------------------------------
public class Tile_ClientVisual : MonoBehaviour
{
    [Header("Références requises")]
    [SerializeField] private Setup_NetworkTile networkTile; // Référence au composant réseau
    [SerializeField] private Renderer tileRenderer;         // Renderer pour modifier le matériau

    [Header("Matériaux assignés")]
    [SerializeField] private Material matNone;
    [SerializeField] private Material matTeamGreen;
    [SerializeField] private Material matTeamRed;
    [SerializeField] private Material matObstacle;
    [SerializeField] private Material matFighterActif;
    [SerializeField] private Material matCursorIndicator;

    // État local de la souris
    private bool isMouseOver = false;

    //------------------------------------------------------------
    private void Start()
    {
        UpdateMaterial();
    }

    //------------------------------------------------------------
    private void Update()
    {
        // En cas de mise à jour dynamique (ex: isFighterActif changé à runtime)
        if (!isMouseOver)
        {
            UpdateMaterial();
        }
    }

    //------------------------------------------------------------
    private void OnMouseEnter()
    {
        isMouseOver = true;

        // Applique un surlignage si possible
        if (!networkTile.isFighterActif && matCursorIndicator != null)
        {
            tileRenderer.material = matCursorIndicator;
        }
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        UpdateMaterial(); // Revenir à l’état logique
    }

    //------------------------------------------------------------
    private void OnMouseDown()
    {
        // Doit être client + joueur local
        if (!NetworkClient.active || NetworkClient.connection == null) return;

        var localPlayerObj = NetworkClient.connection.identity;
        if (localPlayerObj == null)
        {
            Debug.LogWarning("[Tile_ClientVisual] Aucun joueur local trouvé.");
            return;
        }

        Player_ControllerPhasePreparation playerController = localPlayerObj.GetComponent<Player_ControllerPhasePreparation>();
        if (playerController == null)
        {
            Debug.LogWarning("[Tile_ClientVisual] Le joueur local n'a pas de script Player_ControllerPhasePreparation.");
            return;
        }

        playerController.RequestTileClick(networkTile.tileX, networkTile.tileY);
    }

    //------------------------------------------------------------
    // Met à jour le matériau selon l’état logique (sauf si souris dessus)
    private void UpdateMaterial()
    {
        if (tileRenderer == null || networkTile == null) return;

        // Cas spécial : joueur actif → priorité visuelle
        if (networkTile.isFighterActif && matFighterActif != null)
        {
            tileRenderer.material = matFighterActif;
            return;
        }

        // Matériau selon l’état logique réseau
        switch (networkTile.currentState)
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
}
