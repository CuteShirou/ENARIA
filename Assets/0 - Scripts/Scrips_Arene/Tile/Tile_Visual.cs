using UnityEngine;

[AddComponentMenu("Combat/Tile Visual (Local)")]
public class Tile_Visual : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private SetupTile setup;     // Composant logique local (coords + états)
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

    private bool isMouseOver = false;

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
        // Mise à jour continue si pas de survol (ex: changement d'état runtime)
        if (!isMouseOver) UpdateMaterial();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        if (setup != null && !setup.isFighterActif && matCursorIndicator != null && tileRenderer != null)
            tileRenderer.material = matCursorIndicator;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        if (tileRenderer == null || setup == null) return;

        // Priorité visuelle : combattant actif
        if (setup.isFighterActif && matFighterActif != null)
        {
            tileRenderer.material = matFighterActif;
            return;
        }

        // État de la tuile (Team/Obstacle/None)
        switch (setup.currentState)
        {
            case Tile_State.TeamGreen:
                if (matTeamGreen) tileRenderer.material = matTeamGreen;
                break;
            case Tile_State.TeamRed:
                if (matTeamRed) tileRenderer.material = matTeamRed;
                break;
            case Tile_State.Obstacle:
                if (matObstacle) tileRenderer.material = matObstacle;
                break;
            default:
                if (matNone) tileRenderer.material = matNone;
                break;
        }

        // Damier pour None
        if (setup.currentState == Tile_State.None)
        {
            bool isEven = ((setup.tileX + setup.tileY) % 2) == 0;
            if (isEven && matColor1) tileRenderer.material = matColor1;
            else if (!isEven && matColor2) tileRenderer.material = matColor2;
        }
    }
}
