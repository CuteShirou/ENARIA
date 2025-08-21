using UnityEngine;

[AddComponentMenu("Combat/Entity Statistique Combat (Local)")]
public class Entity_StatistiqueCombat : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    // ÉTAT GLOBAL
    [Header("État")]
    public bool isFight = false;

    [SerializeField] private bool _isDead = false;
    public bool isDead
    {
        get => _isDead;
        set { if (_isDead == value) return; bool old = _isDead; _isDead = value; OnDeadChanged(old, _isDead); }
    }

    [Tooltip("0 = Verte (joueurs), 1 = Rouge (monstres)")]
    [SerializeField] private int _team = 0;
    public int team
    {
        get => _team;
        set { if (_team == value) return; int old = _team; _team = value; OnTeamChanged(old, _team); }
    }

    [Header("Préparation")]
    [SerializeField] private bool _isReady = false;
    public bool isReady
    {
        get => _isReady;
        set { if (_isReady == value) return; bool old = _isReady; _isReady = value; OnReadyChanged(old, _isReady); }
    }

    // ─────────────────────────────────────────────────────────────────────
    // STATS BASE (Design)
    [Header("Stats de base (Design)")]
    public int baseHP = 100;
    public int basePA = 6;
    public int basePM = 3;
    public int basePO = 4;

    [Range(0f, 100f)] public float baseCritChance = 0f;

    [Tooltip("Utilisé pour l'ordre de tour (timeline).")]
    public int baseInitiative = 10;

    public int baseForce;
    public int baseDexterite;
    public int baseMagie;
    public int baseFoi;

    [Header("Résistances de base (%)")]
    [Range(-100f, 100f)] public float baseResistanceForce;
    [Range(-100f, 100f)] public float baseResistanceDexterite;
    [Range(-100f, 100f)] public float baseResistanceMagie;
    [Range(-100f, 100f)] public float baseResistanceFoi;

    // ─────────────────────────────────────────────────────────────────────
    // STATS COURANTES (Runtime)
    [Header("Stats courantes (Runtime)")]
    [SerializeField] private int _currentHP;
    [SerializeField] private int _currentPA;
    [SerializeField] private int _currentPM;
    [SerializeField] private int _currentPO;
    [SerializeField] private float _currentCritChance;

    [SerializeField] private int _currentInitiative;

    [SerializeField] private int _currentForce;
    [SerializeField] private int _currentDexterite;
    [SerializeField] private int _currentMagie;
    [SerializeField] private int _currentFoi;

    [SerializeField, Range(-100f, 100f)] private float _currentResistanceForce;
    [SerializeField, Range(-100f, 100f)] private float _currentResistanceDexterite;
    [SerializeField, Range(-100f, 100f)] private float _currentResistanceMagie;
    [SerializeField, Range(-100f, 100f)] private float _currentResistanceFoi;

    // Accesseurs lecture seule (compat)
    public int currentHP => _currentHP;
    public int currentPA => _currentPA;
    public int currentPM => _currentPM;
    public int currentPO => _currentPO;
    public float currentCritChance => _currentCritChance;

    public int currentInitiative => _currentInitiative;

    public int currentForce => _currentForce;
    public int currentDexterite => _currentDexterite;
    public int currentMagie => _currentMagie;
    public int currentFoi => _currentFoi;

    public float currentResistanceForce => _currentResistanceForce;
    public float currentResistanceDexterite => _currentResistanceDexterite;
    public float currentResistanceMagie => _currentResistanceMagie;
    public float currentResistanceFoi => _currentResistanceFoi;

    // ─────────────────────────────────────────────────────────────────────
    // INITIALISATION / TOURS
    /// <summary>Copie toutes les bases vers les "current". À appeler à l'entrée en combat.</summary>
    public void InitStatsFromBase()
    {
        SetHP(baseHP);
        SetPA(basePA);
        SetPM(basePM);
        SetPO(basePO);

        SetCrit(baseCritChance);
        SetInitiative(baseInitiative);

        SetForce(baseForce);
        SetDex(baseDexterite);
        SetMagie(baseMagie);
        SetFoi(baseFoi);

        SetResForce(baseResistanceForce);
        SetResDex(baseResistanceDexterite);
        SetResMagie(baseResistanceMagie);
        SetResFoi(baseResistanceFoi);

        Debug.Log($"[Stats] Init depuis bases pour {name}");
    }

    /// <summary>Réinitialise PA/PM au début d’un tour.</summary>
    public void ResetTurnStats()
    {
        SetPA(basePA);
        SetPM(basePM);
    }

    /// <summary>Inverse le statut prêt (phase de préparation).</summary>
    public void ToggleReady()
    {
        isReady = !isReady;
        Debug.Log($"[Local] {name} → isReady = {isReady}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // SETTERS (avec logs type "hooks")
    public void SetHP(int value)
    {
        value = Mathf.Clamp(value, 0, Mathf.Max(1, baseHP));
        if (value == _currentHP) return;
        int old = _currentHP; _currentHP = value; OnHPChanged(old, _currentHP);
    }

    public void SetPA(int value) { value = Mathf.Max(0, value); if (value == _currentPA) return; int old = _currentPA; _currentPA = value; OnPAChanged(old, _currentPA); }
    public void SetPM(int value) { value = Mathf.Max(0, value); if (value == _currentPM) return; int old = _currentPM; _currentPM = value; OnPMChanged(old, _currentPM); }
    public void SetPO(int value) { value = Mathf.Max(0, value); if (value == _currentPO) return; int old = _currentPO; _currentPO = value; OnPOChanged(old, _currentPO); }

    public void SetCrit(float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Approximately(value, _currentCritChance)) return;
        float old = _currentCritChance; _currentCritChance = value; OnCritChanged(old, _currentCritChance);
    }

    public void SetInitiative(int value)
    {
        value = Mathf.Max(0, value);
        if (value == _currentInitiative) return;
        int old = _currentInitiative; _currentInitiative = value; OnInitiativeChanged(old, _currentInitiative);
    }

    public void SetForce(int value) { if (value == _currentForce) return; int old = _currentForce; _currentForce = value; OnForceChanged(old, _currentForce); }
    public void SetDex(int value) { if (value == _currentDexterite) return; int old = _currentDexterite; _currentDexterite = value; OnDexChanged(old, _currentDexterite); }
    public void SetMagie(int value) { if (value == _currentMagie) return; int old = _currentMagie; _currentMagie = value; OnMagieChanged(old, _currentMagie); }
    public void SetFoi(int value) { if (value == _currentFoi) return; int old = _currentFoi; _currentFoi = value; OnFoiChanged(old, _currentFoi); }

    // Résistances : NEGATIF autorisé → clamp -100..100
    public void SetResForce(float value)
    {
        if (Mathf.Approximately(value, _currentResistanceForce)) return;
        float old = _currentResistanceForce;
        _currentResistanceForce = Mathf.Clamp(value, -100f, 100f);
        OnResForceChanged(old, _currentResistanceForce);
    }
    public void SetResDex(float value)
    {
        if (Mathf.Approximately(value, _currentResistanceDexterite)) return;
        float old = _currentResistanceDexterite;
        _currentResistanceDexterite = Mathf.Clamp(value, -100f, 100f);
        OnResDexChanged(old, _currentResistanceDexterite);
    }
    public void SetResMagie(float value)
    {
        if (Mathf.Approximately(value, _currentResistanceMagie)) return;
        float old = _currentResistanceMagie;
        _currentResistanceMagie = Mathf.Clamp(value, -100f, 100f);
        OnResMagieChanged(old, _currentResistanceMagie);
    }
    public void SetResFoi(float value)
    {
        if (Mathf.Approximately(value, _currentResistanceFoi)) return;
        float old = _currentResistanceFoi;
        _currentResistanceFoi = Mathf.Clamp(value, -100f, 100f);
        OnResFoiChanged(old, _currentResistanceFoi);
    }

    // ─────────────────────────────────────────────────────────────────────
    // HOOKS (logs – remplace SyncVar hooks)
    private void OnHPChanged(int oldVal, int newVal) => Debug.Log($"[Local] HP : {oldVal} → {newVal}");
    private void OnPAChanged(int oldVal, int newVal) => Debug.Log($"[Local] PA : {oldVal} → {newVal}");
    private void OnPMChanged(int oldVal, int newVal) => Debug.Log($"[Local] PM : {oldVal} → {newVal}");
    private void OnPOChanged(int oldVal, int newVal) => Debug.Log($"[Local] PO : {oldVal} → {newVal}");
    private void OnCritChanged(float oldVal, float newVal) => Debug.Log($"[Local] Crit% : {oldVal} → {newVal}");
    private void OnInitiativeChanged(int oldVal, int newVal) => Debug.Log($"[Local] Initiative : {oldVal} → {newVal}");

    private void OnForceChanged(int oldVal, int newVal) => Debug.Log($"[Local] Force : {oldVal} → {newVal}");
    private void OnDexChanged(int oldVal, int newVal) => Debug.Log($"[Local] Dextérité : {oldVal} → {newVal}");
    private void OnMagieChanged(int oldVal, int newVal) => Debug.Log($"[Local] Magie : {oldVal} → {newVal}");
    private void OnFoiChanged(int oldVal, int newVal) => Debug.Log($"[Local] Foi : {oldVal} → {newVal}");

    private void OnResForceChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Force : {oldVal}% → {newVal}%");
    private void OnResDexChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Dextérité : {oldVal}% → {newVal}%");
    private void OnResMagieChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Magie : {oldVal}% → {newVal}%");
    private void OnResFoiChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Foi : {oldVal}% → {newVal}%");

    private void OnTeamChanged(int oldVal, int newVal) => Debug.Log($"[Local] Team : {oldVal} → {newVal}");
    private void OnReadyChanged(bool oldVal, bool newVal) => Debug.Log($"[Local] Ready : {oldVal} → {newVal}");
    private void OnDeadChanged(bool oldVal, bool newVal) => Debug.Log($"[Local] isDead : {oldVal} → {newVal} ({name})");
}
