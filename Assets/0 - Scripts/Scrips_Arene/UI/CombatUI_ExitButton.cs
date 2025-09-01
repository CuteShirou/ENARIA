using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Combat/UI/Exit Button")]
public class CombatUI_ExitButton : MonoBehaviour
{
    [SerializeField] private Combat_PhaseManager combatManager;
    [SerializeField] private bool allowDuringPreparation = true;
    [SerializeField] private bool allowDuringTurnByTurn = true;

    private Button btn;


    private void OnEnable() => RefreshInteractable();
    private void Update() => RefreshInteractable();

    private void RefreshInteractable()
    {
        if (btn == null || combatManager == null) return;

        var phase = combatManager.GetCurrentPhase();
        bool can =
            (allowDuringPreparation && phase == CombatPhase.Preparation) ||
            (allowDuringTurnByTurn && phase == CombatPhase.TurnByTurn);

        btn.interactable = can;
    }

    // À binder dans l'événement OnClick du bouton
    public void OnClick_ExitLose()
    {
        if (combatManager == null) return;
        // Abandon → combat perdu
        combatManager.ForceEndPhase(false);
    }
}
