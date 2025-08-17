using UnityEngine;

[AddComponentMenu("Combat/Phase - End Combat (Local)")]
public class Phase_EndCombat : MonoBehaviour
{
    [Header("UI (Scène unique)")]
    [SerializeField] private GameObject explorationUIRoot; // ex: Exploration_UI
    [SerializeField] private GameObject combatUIRoot;      // ex: Combat_UI

    private Combat_PhaseManager manager;

    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        // Combat → OFF, Exploration → ON
        if (combatUIRoot != null) combatUIRoot.SetActive(false);
        if (explorationUIRoot != null) explorationUIRoot.SetActive(true);

        Debug.Log("[End] Fin de combat : retour UI d'exploration.");
        // Ici, tu peux nettoyer/réinitialiser d’autres états si besoin (TP retour, etc.)
    }
}
