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
    [SerializeField] private Mode startupMode = Mode.Exploration;   // Mode au démarrage
    [SerializeField] private bool applyModeOnAwake = true;          // Appliquer dès Awake
    [SerializeField] private bool reapplyOnStart = true;            // Ré-appliquer en Start (assure l'état après tous les Awake)

    [Header("Scripts à activer par mode")]
    [SerializeField] private List<Behaviour> explorationScripts = new();   // Ex: ThirdPersonController, OnClick3D
    [SerializeField] private List<Behaviour> preparationScripts = new();   // Ex: Player_Controller Phase Preparation
    [SerializeField] private List<Behaviour> combatScripts = new();        // Ex: Player_Combat Controller

    [Header("GameObjects à activer par mode (optionnel)")]
    [SerializeField] private List<GameObject> explorationObjects = new();
    [SerializeField] private List<GameObject> preparationObjects = new();
    [SerializeField] private List<GameObject> combatObjects = new();

    public Mode CurrentMode { get; private set; }

    void Awake()
    {
        // Tout OFF au tout début pour éviter les chevauchements d'inputs
        SetAllEnabled(explorationScripts, false);
        SetAllEnabled(preparationScripts, false);
        SetAllEnabled(combatScripts, false);
        SetAllActive(explorationObjects, false);
        SetAllActive(preparationObjects, false);
        SetAllActive(combatObjects, false);

        // Choix du mode de départ
        CurrentMode = startupMode;

        // Application immédiate si souhaité
        if (applyModeOnAwake) ApplyMode(CurrentMode);
    }

    void Start()
    {
        // Ré-applique après tous les Awake (fiabilise l'état effectif au lancement)
        if (reapplyOnStart) ApplyMode(CurrentMode);
    }

    public void SetMode(Mode newMode)
    {
        // Change le mode courant et applique
        if (CurrentMode != newMode) CurrentMode = newMode;
        ApplyMode(CurrentMode);
    }

    public void SetExploration() => SetMode(Mode.Exploration);
    public void SetPreparationCombat() => SetMode(Mode.PreparationCombat);
    public void SetTurnByTurnCombat() => SetMode(Mode.TurnByTurnCombat);

    void ApplyMode(Mode mode)
    {
        // Tout OFF
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

    static void SetAllEnabled(List<Behaviour> list, bool enabled)
    {
        // Active/désactive tous les scripts référencés
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
            if (list[i]) list[i].enabled = enabled;
    }

    static void SetAllActive(List<GameObject> list, bool active)
    {
        // Active/désactive tous les GameObjects référencés
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
            if (list[i]) list[i].SetActive(active);
    }

#if UNITY_EDITOR
    [ContextMenu("Mode -> Exploration")] void Ctx_Exploration() => SetExploration();
    [ContextMenu("Mode -> PreparationCombat")] void Ctx_Preparation() => SetPreparationCombat();
    [ContextMenu("Mode -> TurnByTurnCombat")] void Ctx_Combat() => SetTurnByTurnCombat();
#endif
}
