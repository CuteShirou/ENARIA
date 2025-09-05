using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI_ReadyButton : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Combat_PhaseManager combatManager;
    [SerializeField] private Entity_StatistiqueCombat playerStats;

    [Header("Comportement")]
    [Tooltip("Refuse de toggler si on n'est pas en phase Préparation.")]
    [SerializeField] private bool onlyDuringPreparation = true;

    private Button btn;

    private void Awake()
    {
        RefreshUI();
    }

    private void OnEnable() => RefreshUI();

    public void OnClick_ToggleReady()
    {
        if (playerStats == null) { Debug.LogWarning("[UI] Aucun Entity_StatistiqueCombat joueur assigné."); return; }
        if (onlyDuringPreparation && (combatManager == null || combatManager.GetCurrentPhase() != CombatPhase.Preparation))
        {
            Debug.Log("[UI] Bouton 'Prêt' ignoré (hors phase Préparation).");
            return;
        }

        playerStats.ToggleReady();     // inverse isReady (true/false)
        RefreshUI();
        TryAdvanceIfAllReady();        // passe direct en TourParTour si tout le monde est prêt
    }

    private void RefreshUI()
    {
        if (btn != null) btn.interactable = playerStats != null;
    }

    private void TryAdvanceIfAllReady()
    {
        if (combatManager == null || combatManager.GetCurrentPhase() != CombatPhase.Preparation) return;
        var enter = combatManager.phaseEnter;
        if (enter == null || enter.AllFighters == null || enter.AllFighters.Count == 0) return;

        foreach (var go in enter.AllFighters)
        {
            if (go == null) continue;
            if (!go.TryGetComponent(out Entity_StatistiqueCombat stats) || !stats.isReady)
                return; // quelqu’un n’est pas prêt → on sort
        }

        Debug.Log("[UI] Tous prêts via bouton → Passage TourParTour.");
        combatManager.StartPhase(CombatPhase.TurnByTurn);
    }
}
