using UnityEngine;

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
    [SerializeField] private Material matZoneImpact;      // Matériau pour l’aperçu de zone d’impact
    [SerializeField] private Material matCursorIndicator;

    // Injecté par la grille
    private TileGrid_Manager tileGrid;     // Référence vers la grille
    private InfoEntityPanelUI infoPanel;   // Référence vers la bulle d’info

    private bool isMouseOver = false;

    // état local pour savoir si la tuile fait partie de l’aperçu d’impact
    private bool isImpactPreview = false;

    // --- SetShared ---------------------------------------------------------
    // Appelée par la grille pour injecter les références de scène.
    public void SetShared(TileGrid_Manager grid, InfoEntityPanelUI panel)
    {
        tileGrid = grid;
        infoPanel = panel;
    }

    // --- SetImpactPreview --------------------------------------------------
    // Active/désactive le mode “zone d’impact” sur cette tuile.
    // Prioritaire par rapport au curseur de survol.
    public void SetImpactPreview(bool enabled)
    {
        isImpactPreview = enabled; // Mémorise l’état
        UpdateMaterial();          // Rafraîchit le rendu
    }

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
        // Si la souris n’est pas dessus, on laisse l’état visuel “vivre”.
        if (!isMouseOver) UpdateMaterial();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;

        // si la tuile est en aperçu d’impact, on NE remplace pas le matériau.
        if (!isImpactPreview)
        {
            if (setup != null && !setup.isFighterActif && matCursorIndicator && tileRenderer)
                tileRenderer.material = matCursorIndicator;
        }

        // Affiche la bulle si occupée
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

    // --- UpdateMaterial ----------------------------------------------------
    // Choisit le matériau selon l’état actuel (aperçu -> actif -> état logique -> damier).
    private void UpdateMaterial()
    {
        if (tileRenderer == null || setup == null) return;

        // Priorité absolue : aperçu de zone d’impact
        if (isImpactPreview && matZoneImpact)
        {
            tileRenderer.material = matZoneImpact;
            return;
        }

        // Surbrillance combattant actif
        if (setup.isFighterActif && matFighterActif)
        {
            tileRenderer.material = matFighterActif;
            return;
        }

        // État logique
        switch (setup.currentState)
        {
            case Tile_State.TeamGreen: if (matTeamGreen) tileRenderer.material = matTeamGreen; break;
            case Tile_State.TeamRed: if (matTeamRed) tileRenderer.material = matTeamRed; break;
            case Tile_State.Obstacle: if (matObstacle) tileRenderer.material = matObstacle; break;
            default: if (matNone) tileRenderer.material = matNone; break;
        }

        // Damier neutre si None
        if (setup.currentState == Tile_State.None)
        {
            bool even = ((setup.tileX + setup.tileY) % 2) == 0;
            if (even && matColor1) tileRenderer.material = matColor1;
            else if (!even && matColor2) tileRenderer.material = matColor2;
        }
    }
}
