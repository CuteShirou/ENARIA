using System.Collections.Generic;
using UnityEngine;

public enum CombatPhase
{
    Enter,
    Preparation,
    TurnByTurn,
    End
}

// Identité logique d'équipe (pour la pop-up à venir)
public enum CombatTeamId
{
    None = 0,
    Green = 1,
    Red = 2
}

[AddComponentMenu("Combat/Combat Phase Manager (Local)")]
public class Combat_PhaseManager : MonoBehaviour
{
    [Header("Index unique de cette arène")]
    [Tooltip("Identifiant logique (ex: 0..9) si tu as plusieurs arènes dans la scène.")]
    public int arenaIndex = 0;

    [Header("Résultat du combat (dernier)")]
    public bool lastCombatWinning = false;  // Flag simple conservé pour compatibilité

    [Header("Références des scripts de phase (Drag & Drop)")]
    public Phase_EnterSetupCombat phaseEnter;
    public Phase_PreparationPlacementCombat phasePrepa;
    public Phase_TurnByTurnCombat phaseTurn;
    public Phase_EndCombat phaseEnd;

    [Header("Grille de l’arène")]
    public TileGrid_Manager tileGrid;

    [Header("Auto-wire (optionnel)")]
    [Tooltip("Si vrai, cherche automatiquement les références manquantes au Awake().")]
    [SerializeField] private bool autoWireIfNull = true;

    // --- Etat courant de la victoire (utilisé pour la pop-up WIN/LOSE) ---
    [Header("Etat courant de la victoire")]
    [Tooltip("Vrai si l'équipe verte est détectée gagnante.")]
    public bool teamGreenIsWinning = false;

    [Tooltip("Vrai si l'équipe rouge est détectée gagnante.")]
    public bool teamRedIsWinning = false;

    [Tooltip("Equipe gagnante de ce combat (None si indéterminé).")]
    public CombatTeamId winnerTeam = CombatTeamId.None;

    // Phase en cours dans cette arène
    private CombatPhase currentPhase;

    public bool isInCombat { get; private set; } = false;

    private void Awake()
    {
        // commentaire FR : auto-récupération des références manquantes si demandé
        if (autoWireIfNull)
        {
            if (phaseEnter == null) phaseEnter = GetComponentInChildren<Phase_EnterSetupCombat>(true);
            if (phasePrepa == null) phasePrepa = GetComponentInChildren<Phase_PreparationPlacementCombat>(true);
            if (phaseTurn == null) phaseTurn = GetComponentInChildren<Phase_TurnByTurnCombat>(true);
            if (phaseEnd == null) phaseEnd = GetComponentInChildren<Phase_EndCombat>(true);
            if (tileGrid == null) tileGrid = GetComponentInChildren<TileGrid_Manager>(true);
        }

        // commentaire FR : désactiver toutes les phases au démarrage
        SafeEnablePhase(phaseEnter, false);
        SafeEnablePhase(phasePrepa, false);
        SafeEnablePhase(phaseTurn, false);
        SafeEnablePhase(phaseEnd, false);

        isInCombat = false;

        // commentaire FR : reset des états de victoire
        teamGreenIsWinning = false;
        teamRedIsWinning = false;
        winnerTeam = CombatTeamId.None;
    }

    //------------------------------------------------------------
    // Fonction appelée depuis l’exploration (ou setup) pour démarrer un combat
    public void LaunchCombat()
    {
        Debug.Log($"[CombatManager][Arena {arenaIndex}] Initialisation. Démarrage du combat...");
        StartPhase(CombatPhase.Enter);
    }

    //------------------------------------------------------------
    // Active uniquement la phase demandée et appelle son Init
    public void StartPhase(CombatPhase phase)
    {
        currentPhase = phase;

        isInCombat = (phase == CombatPhase.Enter ||
                      phase == CombatPhase.Preparation ||
                      phase == CombatPhase.TurnByTurn);

        SafeEnablePhase(phaseEnter, phase == CombatPhase.Enter);
        SafeEnablePhase(phasePrepa, phase == CombatPhase.Preparation);
        SafeEnablePhase(phaseTurn, phase == CombatPhase.TurnByTurn);
        SafeEnablePhase(phaseEnd, phase == CombatPhase.End);

        Debug.Log($"[CombatManager][Arena {arenaIndex}] Changement de phase → {phase}");

        switch (phase)
        {
            case CombatPhase.Enter:
                Debug.Log("--------------------------------------------------");
                Debug.Log("[CombatManager] → Phase_EnterSetupCombat activée.");
                phaseEnter?.InitPhase(this);
                break;

            case CombatPhase.Preparation:
                Debug.Log("--------------------------------------------------");
                Debug.Log("[CombatManager] → Phase_PreparationPlacementCombat activée.");
                phasePrepa?.InitPhase(this);
                break;

            case CombatPhase.TurnByTurn:
                Debug.Log("--------------------------------------------------");
                Debug.Log("[CombatManager] → Phase_TurnByTurnCombat activée.");
                phaseTurn?.InitPhase(this);
                break;

            case CombatPhase.End:
                isInCombat = false;
                Debug.Log("--------------------------------------------------");
                Debug.Log("[CombatManager] → Phase_EndCombat activée.");
                phaseEnd?.InitPhase(this);
                break;
        }
    }

    // Enchaîne la phase logique suivante
    public void NextPhase()
    {
        Debug.Log($"[CombatManager][Arena {arenaIndex}] Passage à la phase suivante depuis {currentPhase}");

        if (currentPhase == CombatPhase.Enter)
            StartPhase(CombatPhase.Preparation);
        else if (currentPhase == CombatPhase.Preparation)
            StartPhase(CombatPhase.TurnByTurn);
        else if (currentPhase == CombatPhase.TurnByTurn)
            StartPhase(CombatPhase.End);
        else
            Debug.LogWarning("[CombatManager] Aucune phase suivante, combat déjà terminé.");
    }

    // Force l'entrée en phase End (flag simple conservé)
    public void ForceEndPhase(bool isWinning)
    {
        lastCombatWinning = isWinning;
        StartPhase(CombatPhase.End);
    }

    // Accès lecture de la phase
    public CombatPhase GetCurrentPhase() => currentPhase;
    public string GetPhaseName() => currentPhase.ToString();

    //------------------------------------------------------------
    // ----------- Détection Win / Lose (utilise phaseEnter.greenTeam/redTeam) -----------
    public bool TryEvaluateEndOfCombat()
    {
        // uniquement pendant la phase tour-par-tour
        if (currentPhase != CombatPhase.TurnByTurn) return false;

        // récupérer les listes depuis Phase_EnterSetupCombat
        List<GameObject> teamGreen = phaseEnter != null ? phaseEnter.greenTeam : null;
        List<GameObject> teamRed = phaseEnter != null ? phaseEnter.redTeam : null;

        // si indisponibles ou vides, on considère la détection non-applicable
        if (teamGreen == null || teamRed == null) return false;
        if (teamGreen.Count == 0 || teamRed.Count == 0) return false;

        bool greenAlive = HasAnyAlive(teamGreen);
        bool redAlive = HasAnyAlive(teamRed);

        if (!greenAlive && redAlive)
        {
            // Rouge gagne
            winnerTeam = CombatTeamId.Red;
            teamRedIsWinning = true;
            teamGreenIsWinning = false;

            Debug.Log($"[CombatManager][Arena {arenaIndex}] Fin de combat détectée → Vainqueur: RED.");
            ForceEndPhase(false);
            return true;
        }
        if (!redAlive && greenAlive)
        {
            // Vert gagne
            winnerTeam = CombatTeamId.Green;
            teamGreenIsWinning = true;
            teamRedIsWinning = false;

            Debug.Log($"[CombatManager][Arena {arenaIndex}] Fin de combat détectée → Vainqueur: GREEN.");
            ForceEndPhase(true);
            return true;
        }
        return false;
    }

    private bool HasAnyAlive(List<GameObject> team)
    {
        if (team == null || team.Count == 0) return false;

        for (int i = 0; i < team.Count; i++)
        {
            var go = team[i];
            if (!go) continue; // null/détruit = KO

            if (go.TryGetComponent(out Entity_StatistiqueCombat stats))
            {
                // PV strictement positifs = vivant
                if (stats.currentHP > 0) return true;
            }
        }
        return false;
    }

    //------------------------------------------------------------
    private static void SafeEnablePhase(MonoBehaviour phaseBehaviour, bool enable)
    {
        if (phaseBehaviour != null) phaseBehaviour.enabled = enable;
    }
}
