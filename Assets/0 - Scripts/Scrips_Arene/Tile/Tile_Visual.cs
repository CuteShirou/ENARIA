using UnityEngine;

[AddComponentMenu("Combat/Tile Visual (Local)")]
public class Tile_Visual : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private SetupTile setup;
    [SerializeField] private Renderer tileRenderer;

    [Header("Matériaux")]
    [SerializeField] private Material matNone;
    [SerializeField] private Material matColor1;
    [SerializeField] private Material matColor2;
    [SerializeField] private Material matTeamGreen;
    [SerializeField] private Material matTeamRed;
    [SerializeField] private Material matObstacle;
    [SerializeField] private Material matFighterActif;
    [SerializeField] private Material matCursorIndicator;

    //   Injecté par la grille (pas sérialisé, aucune assignation sur le prefab)
    private TileGrid_Manager tileGrid;           // AJOUT
    private InfoEntityPanelUI infoPanel;         // AJOUT

    //   Appelée par la grille pour injecter les refs de scène (AJOUT)
    public void SetShared(TileGrid_Manager grid, InfoEntityPanelUI panel)
    {
        tileGrid = grid;
        infoPanel = panel;
    }

    private bool isMouseOver = false;

    public Tile_Visual() { }
    ~Tile_Visual() { }

    private void Reset()
    {
        if (!setup) setup = GetComponent<SetupTile>();
        if (!tileRenderer) tileRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        if (!setup) setup = GetComponent<SetupTile>();
        if (!tileRenderer) tileRenderer = GetComponentInChildren<Renderer>();
        UpdateMaterial();
    }

    private void Update()
    {
        if (!isMouseOver) UpdateMaterial();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;

        if (setup != null && !setup.isFighterActif && matCursorIndicator && tileRenderer)
            tileRenderer.material = matCursorIndicator;

        //   Si occupée → afficher la bulle, sinon cacher
        if (tileGrid != null && infoPanel != null && setup != null)
        {
            GameObject occ = tileGrid.GetEntityOnTile(setup.gameObject);
            if (occ != null) infoPanel.ShowFor(occ);
            else infoPanel.Hide();
        }
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        UpdateMaterial();

        if (infoPanel != null) infoPanel.Hide();
    }

    private void UpdateMaterial()
    {
        if (tileRenderer == null || setup == null) return;

        if (setup.isFighterActif && matFighterActif)
        {
            tileRenderer.material = matFighterActif;
            return;
        }

        switch (setup.currentState)
        {
            case Tile_State.TeamGreen: if (matTeamGreen) tileRenderer.material = matTeamGreen; break;
            case Tile_State.TeamRed: if (matTeamRed) tileRenderer.material = matTeamRed; break;
            case Tile_State.Obstacle: if (matObstacle) tileRenderer.material = matObstacle; break;
            default: if (matNone) tileRenderer.material = matNone; break;
        }

        if (setup.currentState == Tile_State.None)
        {
            bool even = ((setup.tileX + setup.tileY) % 2) == 0;
            if (even && matColor1) tileRenderer.material = matColor1;
            else if (!even && matColor2) tileRenderer.material = matColor2;
        }
    }
}
