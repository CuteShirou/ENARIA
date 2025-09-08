using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Player_ScriptManager : MonoBehaviour
{
    public enum Mode
    {
        Exploration,
        PreparationCombat,
        TurnByTurnCombat
    }

    [Header("Mode courant")]
    [SerializeField] private Mode startupMode = Mode.Exploration;
    [SerializeField] private bool applyModeOnAwake = true;

    [Header("Scripts à activer par mode")]
    [SerializeField] private List<Behaviour> explorationScripts = new();
    [SerializeField] private List<Behaviour> preparationScripts = new();
    [SerializeField] private List<Behaviour> combatScripts = new();

    [Header("GameObjects à activer par mode (optionnel)")]
    [SerializeField] private List<GameObject> explorationObjects = new();
    [SerializeField] private List<GameObject> preparationObjects = new();
    [SerializeField] private List<GameObject> combatObjects = new();

    public Mode CurrentMode { get; private set; }

    private void Awake()
    {
        // Tout OFF
        SetAllEnabled(explorationScripts, false);
        SetAllEnabled(preparationScripts, false);
        SetAllEnabled(combatScripts, false);
        SetAllActive(explorationObjects, false);
        SetAllActive(preparationObjects, false);
        SetAllActive(combatObjects, false);

        CurrentMode = startupMode;
        if (applyModeOnAwake) ApplyMode(CurrentMode);
    }

    public void SetMode(Mode newMode)
    {
        if (CurrentMode != newMode) CurrentMode = newMode;
        ApplyMode(CurrentMode);
    }

    public void SetExploration() => SetMode(Mode.Exploration);
    public void SetPreparationCombat() => SetMode(Mode.PreparationCombat);
    public void SetTurnByTurnCombat() => SetMode(Mode.TurnByTurnCombat);

    private void ApplyMode(Mode mode)
    {
        // OFF partout
        SetAllEnabled(explorationScripts, false);
        SetAllEnabled(preparationScripts, false);
        SetAllEnabled(combatScripts, false);
        SetAllActive(explorationObjects, false);
        SetAllActive(preparationObjects, false);
        SetAllActive(combatObjects, false);

        // ON le bon groupe
        switch (mode)
        {
            case Mode.Exploration:
                SetAllEnabled(explorationScripts, true);
                SetAllActive(explorationObjects, true);
                break;
            case Mode.PreparationCombat:
                SetAllEnabled(preparationScripts, true);
                SetAllActive(preparationObjects, true);
                break;
            case Mode.TurnByTurnCombat:
                SetAllEnabled(combatScripts, true);
                SetAllActive(combatObjects, true);
                break;
        }
    }

    private static void SetAllEnabled(List<Behaviour> list, bool enabled)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
            if (list[i]) list[i].enabled = enabled;
    }

    private static void SetAllActive(List<GameObject> list, bool active)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
            if (list[i]) list[i].SetActive(active);
    }

#if UNITY_EDITOR
    [ContextMenu("Mode -> Exploration")] private void Ctx_Exploration() => SetExploration();
    [ContextMenu("Mode -> PreparationCombat")] private void Ctx_Preparation() => SetPreparationCombat();
    [ContextMenu("Mode -> TurnByTurnCombat")] private void Ctx_Combat() => SetTurnByTurnCombat();
#endif
}
