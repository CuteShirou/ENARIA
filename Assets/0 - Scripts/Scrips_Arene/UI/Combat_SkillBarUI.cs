using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// Combat_SkillBarUI
/// [FR] Génère des boutons (icônes uniquement) à partir des skills de l'entité
///      et équipe la compétence cliquée dans l'Entity_SkillCaster.
/// </summary>
public class Combat_SkillBarUI : MonoBehaviour
{
    // =========================
    // Constructor / Destructor
    // =========================
    public Combat_SkillBarUI() { /* Constructeur */ }
    ~Combat_SkillBarUI() { /* Déconstructeur (non utilisé) */ }

    [Header("Références")]
    public Entity_StatistiqueCombat ownerStats;  // [FR] L'entité qui possède la liste skillBook
    public Entity_SkillCaster ownerCaster;       // [FR] Le lanceur de sorts (reçoit equippedSkill)

    [Header("UI")]
    public Transform buttonContainer;            // [FR] Parent (ton panel SkillBar avec un Layout Group)
    public Button buttonPrefab;                  // [FR] Ton Prefab_ButtonSkill (Image + Button)

    private readonly List<Button> spawnedButtons = new List<Button>();

    private void Awake()
    {
        // [FR] Auto-bind si non assigné dans l’inspector
        if (!ownerStats) ownerStats = GetComponentInParent<Entity_StatistiqueCombat>();
        if (!ownerCaster) ownerCaster = GetComponentInParent<Entity_SkillCaster>();
        if (!buttonContainer) buttonContainer = transform;
    }

    private void OnEnable()
    {
        // [FR] Rebuild auto lorsque l’UI s’active (tu peux l’appeler manuellement sinon)
        BuildFromOwner();
    }

    /// <summary>
    /// [FR] Construit la barre depuis la liste skillBook de l’entité.
    /// </summary>
    public void BuildFromOwner()
    {
        ClearButtons();

        if (!ownerStats || !ownerCaster || !buttonContainer || !buttonPrefab)
        {
            Debug.LogWarning("[SkillBarUI] Références manquantes (ownerStats/ownerCaster/buttonContainer/buttonPrefab).");
            return;
        }

        var book = ownerStats.skillBook;
        if (book == null || book.Count == 0)
        {
            Debug.Log("[SkillBarUI] Aucun skill dans skillBook.");
            return;
        }

        for (int i = 0; i < book.Count; i++)
        {
            Data_Skill data = book[i];
            if (!data) continue;

            Button btn = Instantiate(buttonPrefab, buttonContainer);
            spawnedButtons.Add(btn);

            // [FR] Image racine du bouton (icône)
            var img = btn.GetComponent<Image>();
            if (img)
            {
                img.sprite = data.icon;          // [FR] Icône du skill
                img.preserveAspect = true;       // [FR] Conserve le ratio
                // [FR] Optionnel : taille native → décommente si tu préfères
                // if (data.icon) img.SetNativeSize();
            }

            // [FR] Désactive tout texte éventuel (UI Text ou TMP_Text) → image-only
            var legacyText = btn.GetComponentInChildren<Text>(true);
            if (legacyText) legacyText.gameObject.SetActive(false);
            var tmpText = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmpText) tmpText.gameObject.SetActive(false);

            // [FR] Listener : équipe ce skill au clic
            btn.onClick.AddListener(() =>
            {
                ownerCaster.equippedSkill = data;     // [FR] Équipe la compétence (direct, pas de refonte)
                Highlight(btn);                       // [FR] Feedback visuel simple
            });
        }

        // [FR] Option : équipe automatiquement le premier skill
        if (spawnedButtons.Count > 0)
            spawnedButtons[0].onClick.Invoke();
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
