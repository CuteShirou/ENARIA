using UnityEngine;
using UnityEngine.UI;       // pour Button
using TMPro;                // <- TextMesh Pro

[AddComponentMenu("Combat/UI/Ready Button (Preparation)")]
public class CombatUI_ReadyButton : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Combat_PhaseManager combatManager;       // Drag & Drop (ou auto-find)
    [SerializeField] private Entity_StatistiqueCombat playerStats;    // Drag & Drop (ou auto-find)
    [SerializeField] private TMP_Text labelTMP;                        // TMP_Text du bouton

    [Header("Libellés")]
    [SerializeField] private string textWhenNotReady = "Prêt";
    [SerializeField] private string textWhenReady = "Pas prêt";

    [Header("Comportement")]
    [Tooltip("Refuse de toggler si on n'est pas en phase Préparation.")]
    [SerializeField] private bool onlyDuringPreparation = true;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (combatManager == null)
            combatManager = FindAnyObjectByType<Combat_PhaseManager>(FindObjectsInactive.Exclude);

        if (playerStats == null)
        {
            // 1) via le contrôleur joueur (si présent)
            var controller = FindAnyObjectByType<Player_ControllerPhasePreparation>(FindObjectsInactive.Exclude);
            if (controller != null) controller.TryGetComponent(out playerStats);

            // 2) sinon, premier combattant d'équipe verte trouvé
#if UNITY_2023_1_OR_NEWER
            if (playerStats == null)
            {
                var allStats = Object.FindObjectsByType<Entity_StatistiqueCombat>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var s in allStats) { if (s != null && s.team == 0) { playerStats = s; break; } }
            }
#else
            if (playerStats == null)
            {
                var allStats = FindObjectsOfType<Entity_StatistiqueCombat>(true);
                foreach (var s in allStats) { if (s != null && s.team == 0) { playerStats = s; break; } }
            }
#endif
        }

        if (labelTMP == null)
            labelTMP = GetComponentInChildren<TMP_Text>(true);

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
        if (labelTMP != null && playerStats != null)
            labelTMP.text = playerStats.isReady ? textWhenReady : textWhenNotReady;
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
