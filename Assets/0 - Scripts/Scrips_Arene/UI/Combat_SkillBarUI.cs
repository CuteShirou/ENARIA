using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Combat_SkillBarUI
/// [FR] Génère des boutons (icônes uniquement) à partir du SkillBook de l'entité
///      et équipe la compétence cliquée dans l'Entity_SkillCaster.
///      Le SkillBook est désormais une liste de Skill_Binding (Skill + FX).
/// </summary>
public class Combat_SkillBarUI : MonoBehaviour
{
    // =========================
    // Constructor / Destructor
    // =========================
    public Combat_SkillBarUI() { /* Constructeur */ }
    ~Combat_SkillBarUI() { /* Déconstructeur (non utilisé) */ }

    [Header("Références")]
    public Entity_StatistiqueCombat ownerStats;   // [FR] L'entité qui possède le SkillBook (List<Skill_Binding>)
    public Entity_SkillCaster ownerCaster;        // [FR] Le lanceur de sorts (reçoit equippedSkill)

    [Header("UI")]
    public Transform buttonContainer;             // [FR] Parent (ton panel SkillBar avec un Layout Group)
    public Button buttonPrefab;                   // [FR] Ton Prefab_ButtonSkill (Image + Button)

    // [FR] Boutons instanciés
    private readonly List<Button> spawnedButtons = new List<Button>();

    // [FR] Binding actuellement sélectionné (utilisable par les contrôleurs pour le FX)
    private Skill_Binding _selectedBinding = null;
    public Skill_Binding SelectedBinding => _selectedBinding;

    private void Awake()
    {
        // [FR] Auto-bind si non assigné dans l’inspector
        if (!ownerStats) ownerStats = GetComponentInParent<Entity_StatistiqueCombat>();
        if (!ownerCaster) ownerCaster = GetComponentInParent<Entity_SkillCaster>();
        if (!buttonContainer) buttonContainer = transform;
    }

    private void OnEnable()
    {
        // [FR] Reconstruit la barre lorsque l’UI s’active
        BuildFromOwner();
    }

    /// <summary>
    /// [FR] Construit la barre depuis le SkillBook (List<Skill_Binding>) de l’entité.
    /// </summary>
    public void BuildFromOwner()
    {
        ClearButtons();
        _selectedBinding = null;

        if (!ownerStats || !ownerCaster || !buttonContainer || !buttonPrefab)
        {
            Debug.LogWarning("[SkillBarUI] Références manquantes (ownerStats/ownerCaster/buttonContainer/buttonPrefab).");
            return;
        }

        var book = ownerStats.skillBook; // [FR] List<Skill_Binding>
        if (book == null || book.Count == 0)
        {
            Debug.Log("[SkillBarUI] Aucun skill dans le SkillBook.");
            return;
        }

        for (int i = 0; i < book.Count; i++)
        {
            Skill_Binding binding = book[i];
            if (binding == null || binding.skill == null) continue;

            Data_Skill data = binding.skill;

            Button btn = Instantiate(buttonPrefab, buttonContainer);
            spawnedButtons.Add(btn);

            // [FR] Image racine du bouton (icône)
            var img = btn.GetComponent<Image>();
            if (img)
            {
                img.sprite = data.icon;        // [FR] Icône du skill
                img.preserveAspect = true;     // [FR] Conserve le ratio
                // if (data.icon) img.SetNativeSize(); // [FR] Option : taille native
            }

            // [FR] Désactive tout texte éventuel (UI Text ou TMP_Text) → image-only
            var legacyText = btn.GetComponentInChildren<Text>(true);
            if (legacyText) legacyText.gameObject.SetActive(false);
            var tmpText = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmpText) tmpText.gameObject.SetActive(false);

            // [FR] Capture locale pour le listener
            Skill_Binding captured = binding;

            // [FR] Listener : équipe CE skill + mémorise le binding sélectionné
            btn.onClick.AddListener(() =>
            {
                ownerCaster.equippedSkill = captured.skill; // [FR] Compat : on ne change pas Entity_SkillCaster
                _selectedBinding = captured;                // [FR] Le FX lié est maintenant accessible
                Highlight(btn);                             // [FR] Feedback visuel simple
            });
        }

        // [FR] Option : équipe automatiquement le premier binding/skill
        if (spawnedButtons.Count > 0)
            spawnedButtons[0].onClick.Invoke();
    }

    /// <summary>
    /// [FR] Retourne le binding correspondant à un Data_Skill donné (utile si une autre
    ///      partie du code ne connaît que le skill et veut le FX associé).
    /// </summary>
    public Skill_Binding FindBindingForSkill(Data_Skill target)
    {
        if (!ownerStats || ownerStats.skillBook == null || target == null) return null;
        for (int i = 0; i < ownerStats.skillBook.Count; i++)
        {
            var b = ownerStats.skillBook[i];
            if (b != null && b.skill == target) return b;
        }
        return null;
    }

    /// <summary>
    /// [FR] Nettoie les anciens boutons.
    /// </summary>
    public void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i])
                Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }

    /// <summary>
    /// [FR] Feedback : assombrit le bouton actif.
    /// </summary>
    private void Highlight(Button active)
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            var b = spawnedButtons[i];
            if (!b) continue;

            var colors = b.colors;
            bool isActive = b == active;

            colors.normalColor = isActive ? new Color(0.85f, 0.85f, 0.85f) : Color.white;
            colors.highlightedColor = isActive ? new Color(0.85f, 0.85f, 0.85f) : Color.white;
            colors.selectedColor = isActive ? new Color(0.78f, 0.78f, 0.78f) : Color.white;

            b.colors = colors;
        }
    }
}
