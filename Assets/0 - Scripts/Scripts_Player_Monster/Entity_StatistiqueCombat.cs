using UnityEngine;
using System;

[AddComponentMenu("Combat/Entity Statistique Combat (Local)")]
public class Entity_StatistiqueCombat : MonoBehaviour
{
    // ------------------------------------------------------------------
    // État & équipe
    [Header("État de l'entité")]
    public bool isFight = false;

    [SerializeField] private bool _isDead = false;
    public bool isDead
    {
        get => _isDead;
        set { if (_isDead == value) return; bool old = _isDead; _isDead = value; OnDeadChanged(old, _isDead); }
    }

    [SerializeField] private bool _isReady = false;
    public bool isReady
    {
        get => _isReady;
        set { if (_isReady == value) return; bool old = _isReady; _isReady = value; OnReadyChanged(old, _isReady); }
    }

    [Header("Team")]
    [Tooltip("0 = Verte, 1 = Rouge")]
    public int team = 0;

    // ------------------------------------------------------------------
    // Base stats (valeurs d’origine)
    [Header("Base Stats")]
    public int baseHP = 200;
    public int basePA = 7;
    public int basePM = 4;
    public int basePO = 0;
    public int baseInitiative;
    [Range(0, 100)] public float baseCritChance;
    public int baseForce;
    public int baseDexterite;
    public int baseMagie;
    public int baseFoi;

    [Header("Résistances (en %) de base")]
    [Range(0, 100)] public float baseResistanceForce;
    [Range(0, 100)] public float baseResistanceDexterite;
    [Range(0, 100)] public float baseResistanceMagie;
    [Range(0, 100)] public float baseResistanceFoi;

    // ------------------------------------------------------------------
    // Current stats
    [Header("Current Stats (locales)")]
    [SerializeField] private int _currentHP;
    [SerializeField] private int _currentPA;
    [SerializeField] private int _currentPM;
    [SerializeField] private int _currentPO;
    [SerializeField] private float _currentCritChance;
    [SerializeField] private int _currentForce;
    [SerializeField] private int _currentDexterite;
    [SerializeField] private int _currentMagie;
    [SerializeField] private int _currentFoi;

    [SerializeField][Range(0, 100)] private float _currentResistanceForce;
    [SerializeField][Range(0, 100)] private float _currentResistanceDexterite;
    [SerializeField][Range(0, 100)] private float _currentResistanceMagie;
    [SerializeField][Range(0, 100)] private float _currentResistanceFoi;

    // Accesseurs publics (lecture seule) pour rester compatibles
    public int currentHP => _currentHP;
    public int currentPA => _currentPA;
    public int currentPM => _currentPM;
    public int currentPO => _currentPO;
    public float currentCritChance => _currentCritChance;
    public int currentForce => _currentForce;
    public int currentDexterite => _currentDexterite;
    public int currentMagie => _currentMagie;
    public int currentFoi => _currentFoi;
    public float currentResistanceForce => _currentResistanceForce;
    public float currentResistanceDexterite => _currentResistanceDexterite;
    public float currentResistanceMagie => _currentResistanceMagie;
    public float currentResistanceFoi => _currentResistanceFoi;

    // ------------------------------------------------------------------
    // API publique (équivalents des anciennes méthodes côté serveur)

    /// <summary> Initialise les stats courantes à partir des bases. </summary>
    public void InitStatsFromBase()
    {
        SetHP(baseHP);
        SetPA(basePA);
        SetPM(basePM);
        SetPO(basePO);
        SetCrit(baseCritChance);
        SetForce(baseForce);
        SetDex(baseDexterite);
        SetMagie(baseMagie);
        SetFoi(baseFoi);

        SetResForce(baseResistanceForce);
        SetResDex(baseResistanceDexterite);
        SetResMagie(baseResistanceMagie);
        SetResFoi(baseResistanceFoi);

        Debug.Log($"[Stats] Stats initialisées pour {gameObject.name}");
    }

    /// <summary> Réinitialise PA/PM pour le début d’un tour. </summary>
    public void ResetTurnStats()
    {
        SetPA(basePA);
        SetPM(basePM);
    }

    /// <summary> Bascule l’état prêt / pas prêt (remplace l’ancienne CmdToggleReady). </summary>
    public void ToggleReady()
    {
        isReady = !isReady;
        Debug.Log($"[LOCAL] {gameObject.name} → isReady = {isReady}");
    }

    // ------------------------------------------------------------------
    // Helpers de modification (reproduisent les "hooks" SyncVar par logs)

    public void SetHP(int value)
    {
        if (value == _currentHP) return;
        int old = _currentHP;
        _currentHP = Mathf.Max(0, value);
        OnHPChanged(old, _currentHP);
        if (_currentHP <= 0) isDead = true;
    }

    public void AddHP(int delta) => SetHP(_currentHP + delta);
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        SetHP(_currentHP - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        SetHP(Mathf.Min(baseHP, _currentHP + amount));
    }

    public void SetPA(int value)
    {
        if (value == _currentPA) return;
        int old = _currentPA; _currentPA = Mathf.Max(0, value); OnPAChanged(old, _currentPA);
    }
    public bool TrySpendPA(int cost)
    {
        if (_currentPA < cost) return false;
        SetPA(_currentPA - cost);
        return true;
    }

    public void SetPM(int value)
    {
        if (value == _currentPM) return;
        int old = _currentPM; _currentPM = Mathf.Max(0, value); OnPMChanged(old, _currentPM);
    }
    public bool TrySpendPM(int cost)
    {
        if (_currentPM < cost) return false;
        SetPM(_currentPM - cost);
        return true;
    }

    public void SetPO(int value) { if (value == _currentPO) return; int old = _currentPO; _currentPO = Mathf.Max(0, value); OnPOChanged(old, _currentPO); }
    public void SetCrit(float value) { if (Mathf.Approximately(value, _currentCritChance)) return; float old = _currentCritChance; _currentCritChance = Mathf.Clamp(value, 0f, 100f); OnCritChanged(old, _currentCritChance); }
    public void SetForce(int value) { if (value == _currentForce) return; int old = _currentForce; _currentForce = value; OnForceChanged(old, _currentForce); }
    public void SetDex(int value) { if (value == _currentDexterite) return; int old = _currentDexterite; _currentDexterite = value; OnDexChanged(old, _currentDexterite); }
    public void SetMagie(int value) { if (value == _currentMagie) return; int old = _currentMagie; _currentMagie = value; OnMagieChanged(old, _currentMagie); }
    public void SetFoi(int value) { if (value == _currentFoi) return; int old = _currentFoi; _currentFoi = value; OnFoiChanged(old, _currentFoi); }

    public void SetResForce(float value) { if (Mathf.Approximately(value, _currentResistanceForce)) return; float old = _currentResistanceForce; _currentResistanceForce = Mathf.Clamp(value, 0f, 100f); OnResForceChanged(old, _currentResistanceForce); }
    public void SetResDex(float value) { if (Mathf.Approximately(value, _currentResistanceDexterite)) return; float old = _currentResistanceDexterite; _currentResistanceDexterite = Mathf.Clamp(value, 0f, 100f); OnResDexChanged(old, _currentResistanceDexterite); }
    public void SetResMagie(float value) { if (Mathf.Approximately(value, _currentResistanceMagie)) return; float old = _currentResistanceMagie; _currentResistanceMagie = Mathf.Clamp(value, 0f, 100f); OnResMagieChanged(old, _currentResistanceMagie); }
    public void SetResFoi(float value) { if (Mathf.Approximately(value, _currentResistanceFoi)) return; float old = _currentResistanceFoi; _currentResistanceFoi = Mathf.Clamp(value, 0f, 100f); OnResFoiChanged(old, _currentResistanceFoi); }

    // ------------------------------------------------------------------
    // "Hooks" (mêmes logs que tes anciens SyncVar hooks)

    private void OnHPChanged(int oldVal, int newVal) => Debug.Log($"[Local] HP : {oldVal} → {newVal}");
    private void OnPAChanged(int oldVal, int newVal) => Debug.Log($"[Local] PA : {oldVal} → {newVal}");
    private void OnPMChanged(int oldVal, int newVal) => Debug.Log($"[Local] PM : {oldVal} → {newVal}");
    private void OnPOChanged(int oldVal, int newVal) => Debug.Log($"[Local] PO : {oldVal} → {newVal}");
    private void OnCritChanged(float oldVal, float newVal) => Debug.Log($"[Local] Critique : {oldVal}% → {newVal}%");
    private void OnForceChanged(int oldVal, int newVal) => Debug.Log($"[Local] Force : {oldVal} → {newVal}");
    private void OnDexChanged(int oldVal, int newVal) => Debug.Log($"[Local] Dextérité : {oldVal} → {newVal}");
    private void OnMagieChanged(int oldVal, int newVal) => Debug.Log($"[Local] Magie : {oldVal} → {newVal}");
    private void OnFoiChanged(int oldVal, int newVal) => Debug.Log($"[Local] Foi : {oldVal} → {newVal}");

    private void OnResForceChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Force : {oldVal}% → {newVal}%");
    private void OnResDexChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Dextérité : {oldVal}% → {newVal}%");
    private void OnResMagieChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Magie : {oldVal}% → {newVal}%");
    private void OnResFoiChanged(float oldVal, float newVal) => Debug.Log($"[Local] Résistance Foi : {oldVal}% → {newVal}%");

    private void OnDeadChanged(bool oldVal, bool newVal) => Debug.Log($"[Local] isDead : {oldVal} → {newVal} pour {gameObject.name}");
    private void OnReadyChanged(bool oldVal, bool newVal) => Debug.Log($"[Local] isReady : {oldVal} → {newVal} pour {gameObject.name}");
}
