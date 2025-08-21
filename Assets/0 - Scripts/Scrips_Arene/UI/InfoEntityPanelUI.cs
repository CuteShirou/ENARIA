using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau d'infos d'une entité (UI de combat).
/// - Références en Drag & Drop uniquement (aucun Find).
/// - Value01..04 = Résistances : Force, Dextérité, Foi, Magie (floats, négatifs possibles).
/// - PA/PM/PO : valeurs courantes si dispo, sinon 0.
/// - HP : "current / base" + barre Filled Horizontal (Origin Left).
/// - Rafraîchit l'affichage HP en continu quand le panel est visible.
/// </summary>
[AddComponentMenu("Combat/UI/Info Entity Panel UI")]
public class InfoEntityPanelUI : MonoBehaviour
{
    [Header("Racine du panel (laisser vide = ce GO)")]
    [SerializeField] private GameObject panelRoot;

    [Header("En-tête")]
    [SerializeField] private TMP_Text nameText;   // ex: Bubble_Pseudo_Entity/Text_Name_Entity
    [SerializeField] private TMP_Text levelText;  // ex: Bubble_Lvl_Entity/Text_Lvl_Entity
    [SerializeField] private Image portrait;      // ex: InfoBubble_IconPlayer

    [Header("Valeurs (texte uniquement)")]
    [SerializeField] private TMP_Text valuePA;    // Stat_PA/ValuePA
    [SerializeField] private TMP_Text valuePM;    // Stat_PM/ValuePM
    [SerializeField] private TMP_Text valuePO;    // Stat_PO/ValuePO

    // Value01..04 = résistances (ordre demandé)
    [SerializeField] private TMP_Text value01_ResForce;     // Stat_01/Value1
    [SerializeField] private TMP_Text value02_ResDexterite; // Stat_02/Value2
    [SerializeField] private TMP_Text value03_ResFoi;       // Stat_03/Value3
    [SerializeField] private TMP_Text value04_ResMagie;     // Stat_04/Value4

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;     // Stat_HP/ValueHP
    [SerializeField] private Image hpBar;         // HP_Bar/Image_CurrentHP (Image Filled)

    [Header("Options")]
    [SerializeField] private bool autoRefreshHPWhileVisible = true;

    private GameObject currentEntity;
    private Entity_Info info;
    private Entity_StatistiqueCombat stats;

    private void Awake()
    {
        if (!panelRoot) panelRoot = gameObject;
        panelRoot.SetActive(false);

        if (hpBar)
        {
            hpBar.type = Image.Type.Filled;
            hpBar.fillMethod = Image.FillMethod.Horizontal;
            hpBar.fillOrigin = (int)Image.OriginHorizontal.Left; // se vide de droite vers gauche
        }
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
        float v = value ?? 0f;                 // on affiche exactement la current (sinon 0)
        label.text = $"{v:0.##} %";             // 0, 1.5, -12.25, etc. → formaté avec 2 décimales max
    }
}
