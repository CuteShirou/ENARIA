using Mirror;
using UnityEngine;

//------------------------------------------------------------
public class Entity_StatistiqueCombat : NetworkBehaviour
{
    [Header("État de l'entité")]
    [SerializeField] public bool isFight = false;
    [SerializeField] public bool isDead = false;
    [SerializeField] public bool isReady = false;

    [Header("Team")]
    [SyncVar] public int team; // 0 = Verte, 1 = Rouge

    [Header("Base Stats")]
    [SerializeField] public int baseHP = 200;
    [SerializeField] public int basePA = 7;
    [SerializeField] public int basePM = 4;
    [SerializeField] public int basePO = 0;
    [SerializeField] public int baseInitiative;

    [SerializeField][Range(0, 100)] public float baseCritChance;
    [SerializeField] public int baseForce;
    [SerializeField] public int baseDexterite;
    [SerializeField] public int baseMagie;
    [SerializeField] public int baseFoi;

    [Header("Résistances (en %) de base")]
    [SerializeField][Range(0, 100)] public float baseResistanceForce;
    [SerializeField][Range(0, 100)] public float baseResistanceDexterite;
    [SerializeField][Range(0, 100)] public float baseResistanceMagie;
    [SerializeField][Range(0, 100)] public float baseResistanceFoi;

    [Header("Current Stats (synchronisées avec hook)")]
    [SyncVar(hook = nameof(OnHPChanged))] public int currentHP;
    [SyncVar(hook = nameof(OnPAChanged))] public int currentPA;
    [SyncVar(hook = nameof(OnPMChanged))] public int currentPM;
    [SyncVar(hook = nameof(OnPOChanged))] public int currentPO;
    [SyncVar(hook = nameof(OnCritChanged))] public float currentCritChance;
    [SyncVar(hook = nameof(OnForceChanged))] public int currentForce;
    [SyncVar(hook = nameof(OnDexChanged))] public int currentDexterite;
    [SyncVar(hook = nameof(OnMagieChanged))] public int currentMagie;
    [SyncVar(hook = nameof(OnFoiChanged))] public int currentFoi;

    [SyncVar(hook = nameof(OnResForceChanged))][Range(0, 100)] public float currentResistanceForce;
    [SyncVar(hook = nameof(OnResDexChanged))][Range(0, 100)] public float currentResistanceDexterite;
    [SyncVar(hook = nameof(OnResMagieChanged))][Range(0, 100)] public float currentResistanceMagie;
    [SyncVar(hook = nameof(OnResFoiChanged))][Range(0, 100)] public float currentResistanceFoi;

    //------------------------------------------------------------
    // Initialisation des stats depuis les valeurs de base (serveur uniquement)
    [Server]
    public void InitStatsFromBase()
    {
        currentHP = baseHP;
        currentPA = basePA;
        currentPM = basePM;
        currentPO = basePO;
        currentCritChance = baseCritChance;
        currentForce = baseForce;
        currentDexterite = baseDexterite;
        currentMagie = baseMagie;
        currentFoi = baseFoi;

        currentResistanceForce = baseResistanceForce;
        currentResistanceDexterite = baseResistanceDexterite;
        currentResistanceMagie = baseResistanceMagie;
        currentResistanceFoi = baseResistanceFoi;

        Debug.Log($"[Stats] Stats initialisées pour {gameObject.name}");
    }

    //------------------------------------------------------------
    [Server]
    public void ResetTurnStats()
    {
        currentPA = basePA;
        currentPM = basePM;
    }

    //------------------------------------------------------------
    // Hooks déclenchés automatiquement côté client
    private void OnHPChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] HP : {oldVal} → {newVal}");
    private void OnPAChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] PA : {oldVal} → {newVal}");
    private void OnPMChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] PM : {oldVal} → {newVal}");
    private void OnPOChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] PO : {oldVal} → {newVal}");
    private void OnCritChanged(float oldVal, float newVal) => Debug.Log($"[SyncVar] Critique : {oldVal}% → {newVal}%");
    private void OnForceChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] Force : {oldVal} → {newVal}");
    private void OnDexChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] Dextérité : {oldVal} → {newVal}");
    private void OnMagieChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] Magie : {oldVal} → {newVal}");
    private void OnFoiChanged(int oldVal, int newVal) => Debug.Log($"[SyncVar] Foi : {oldVal} → {newVal}");

    private void OnResForceChanged(float oldVal, float newVal) => Debug.Log($"[SyncVar] Résistance Force : {oldVal}% → {newVal}%");
    private void OnResDexChanged(float oldVal, float newVal) => Debug.Log($"[SyncVar] Résistance Dextérité : {oldVal}% → {newVal}%");
    private void OnResMagieChanged(float oldVal, float newVal) => Debug.Log($"[SyncVar] Résistance Magie : {oldVal}% → {newVal}%");
    private void OnResFoiChanged(float oldVal, float newVal) => Debug.Log($"[SyncVar] Résistance Foi : {oldVal}% → {newVal}%");
}
