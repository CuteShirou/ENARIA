using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau d'infos d'une entité (UI de combat).
/// - Rafraîchit l'affichage HP en continu quand le panel est visible.
/// </summary>
public class InfoEntityPanelUI : MonoBehaviour
{
    [Header("Racine du panel (laisser vide = ce GO)")]
    [SerializeField] private GameObject panelRoot;

    [Header("En-tête")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image portrait;

    [Header("Valeurs (texte uniquement)")]
    [SerializeField] private TMP_Text valuePA;
    [SerializeField] private TMP_Text valuePM;
    [SerializeField] private TMP_Text valuePO;

    [SerializeField] private TMP_Text value01_ResForce;
    [SerializeField] private TMP_Text value02_ResDexterite;
    [SerializeField] private TMP_Text value03_ResFoi;
    [SerializeField] private TMP_Text value04_ResMagie;

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image hpBar;

    [Header("Options")]
    [SerializeField] private bool autoRefreshHPWhileVisible = true;

    private GameObject currentEntity;
    private Entity_Info info;
    private Entity_StatistiqueCombat stats;

    private void Awake()
    {
        if (!panelRoot) panelRoot = gameObject;
        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (!autoRefreshHPWhileVisible) return;
        if (!panelRoot || !panelRoot.activeSelf) return;
        if (!stats) return;

        UpdateHPVisuals();
    }

    /// <summary>Affiche et remplit le panneau pour l'entité donnée.</summary>
    public void ShowFor(GameObject entity)
    {
        currentEntity = entity;

        if (!panelRoot || entity == null)
        {
            Hide();
            return;
        }

        info = entity.GetComponent<Entity_Info>();
        stats = entity.GetComponent<Entity_StatistiqueCombat>();

        // --- En-tête ---
        if (nameText)
            nameText.text = info && !string.IsNullOrWhiteSpace(info.entity_Name) ? info.entity_Name : entity.name;

        if (levelText)
            levelText.text = (info && info.entity_Level > 0) ? $"LVL {info.entity_Level}" : "";

        if (portrait)
            portrait.sprite = info ? info.entity_Icon : null;

        // --- PA / PM / PO (courant si dispo sinon 0) ---
        SetInt(valuePA, stats ? stats.currentPA : (int?)null);
        SetInt(valuePM, stats ? stats.currentPM : (int?)null);
        SetInt(valuePO, stats ? stats.currentPO : (int?)null);

        // --- Résistances (floats avec %, négatifs permis) ---
        // On prend la valeur COURANTE telle quelle ; si pas de stats → 0%.
        SetFloatPercent(value01_ResForce, stats ? (float?)stats.currentResistanceForce : null);
        SetFloatPercent(value02_ResDexterite, stats ? (float?)stats.currentResistanceDexterite : null);
        SetFloatPercent(value03_ResFoi, stats ? (float?)stats.currentResistanceFoi : null);
        SetFloatPercent(value04_ResMagie, stats ? (float?)stats.currentResistanceMagie : null);

        // --- HP ---
        UpdateHPVisuals();

        panelRoot.SetActive(true);
    }

    /// <summary>Cache le panneau.</summary>
    public void Hide()
    {
        panelRoot?.SetActive(false);
        currentEntity = null;
        info = null;
        stats = null;
    }

    // ---------------- Helpers d'affichage ----------------

    private void UpdateHPVisuals()
    {
        if (!stats)
        {
            if (hpText) hpText.text = "0 / 0";
            if (hpBar) hpBar.fillAmount = 0f;
            return;
        }

        if (hpText) hpText.text = $"{Mathf.RoundToInt(stats.currentHP)} / {stats.baseHP}";

        if (hpBar)
        {
            float ratio = (stats.baseHP > 0) ? Mathf.Clamp01(stats.currentHP / (float)stats.baseHP) : 0f;
            hpBar.fillAmount = ratio; // Origin Left => se vide depuis la droite quand ça baisse
        }
    }

    private void SetInt(TMP_Text label, int? value)
    {
        if (!label) return;
        label.text = (value ?? 0).ToString();
    }

    private void SetFloatPercent(TMP_Text label, float? value)
    {
        if (!label) return;
        float v = value ?? 0f;
        label.text = $"{v:0.##} %";             // 0, 1.5, -12.25, etc. → formaté avec 2 décimales max
    }
}
