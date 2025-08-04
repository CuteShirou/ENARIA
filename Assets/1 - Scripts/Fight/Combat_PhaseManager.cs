using UnityEngine;

//------------------------------------------------------------
// Enum représentant les différentes phases du combat
public enum CombatPhase
{
    Enter,         // Récupération des données de combat
    Preparation,   // Placement des entités et bouton prêt
    TurnByTurn,    // Tour par tour (combat)
    End            // Fin de combat, retour à l'exploration
}

//------------------------------------------------------------
public class Combat_PhaseManager : MonoBehaviour
{
    [Header("Index unique de cette arène")]
    public int arenaIndex = 0;

    [Header("Références des scripts de phase")]
    public Phase_EnterSetupCombat phaseEnter;
    public Phase_PreparationPlacementCombat phasePrepa;
    public Phase_TurnByTurnCombat phaseTurn;
    public Phase_EndCombat phaseEnd;

    [Header("Grille de l’arène")]
    public TileGrid_Manager tileGrid;

    // Phase en cours dans cette arène
    private CombatPhase currentPhase;

    //------------------------------------------------------------
    // Appelée manuellement par un déclencheur (ex: un joueur qui attaque)
    public void LaunchCombat()
    {
        Debug.Log($"[CombatManager][Arena {arenaIndex}] Initialisation. Démarrage du combat...");
        StartPhase(CombatPhase.Enter);
    }

    //------------------------------------------------------------
    // Lance une phase de combat en activant uniquement le script correspondant
    public void StartPhase(CombatPhase phase)
    {
        currentPhase = phase;

        // Active uniquement la phase demandée, désactive les autres
        phaseEnter.enabled = (phase == CombatPhase.Enter);
        phasePrepa.enabled = (phase == CombatPhase.Preparation);
        phaseTurn.enabled = (phase == CombatPhase.TurnByTurn);
        phaseEnd.enabled = (phase == CombatPhase.End);

        Debug.Log($"[CombatManager][Arena {arenaIndex}] Changement de phase → {phase}");

        // Appelle la méthode InitPhase() correspondante
        switch (phase)
        {
            case CombatPhase.Enter:
                Debug.Log("--------------------------------------------------");
                Debug.Log("[CombatManager] → Phase_EnterSetupCombat activée.");
                phaseEnter.InitPhase(this);
                break;

            case CombatPhase.Preparation:
                Debug.Log("[CombatManager] → Phase_PreparationPlacementCombat activée.");
                phasePrepa.InitPhase(this);
                break;

            //case CombatPhase.TurnByTurn:
            //    Debug.Log("[CombatManager] → Phase_TurnByTurnCombat activée.");
            //    phaseTurn.InitPhase(this);
            //    break;

            //case CombatPhase.End:
            //    Debug.Log("[CombatManager] → Phase_EndCombat activée.");
            //    phaseEnd.InitPhase(this);
            //    break;
        }
    }

    //------------------------------------------------------------
    // Permet d'enchaîner automatiquement la phase suivante dans l'ordre logique
    public void NextPhase()
    {
        Debug.Log($"[CombatManager][Arena {arenaIndex}] Passage à la phase suivante depuis {currentPhase}");

        if (currentPhase == CombatPhase.Enter)
        {
            StartPhase(CombatPhase.Preparation);
        }
        else if (currentPhase == CombatPhase.Preparation)
        {
            StartPhase(CombatPhase.TurnByTurn);
        }
        else if (currentPhase == CombatPhase.TurnByTurn)
        {
            StartPhase(CombatPhase.End);
        }
        else
        {
            Debug.LogWarning("[CombatManager] Aucune phase suivante, combat déjà terminé.");
        }
    }
}
