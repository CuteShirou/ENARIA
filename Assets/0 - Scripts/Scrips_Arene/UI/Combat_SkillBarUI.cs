using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Combat_SkillBarUI
///   - Construit la barre de compétences depuis le SkillBook
///   - Sélectionne la compétence cliquée (ownerCaster.equippedSkill)
///   - Affiche la zone d'impact (MatZoneImpact) sous la souris
///   - Verrouille l'UI quand ce n'est pas le tour du propriétaire (clicks bloqués + preview coupée)
///   - Clic droit : on laisse Entity_SkillCaster caster; on désélectionne au frame suivant
///   - Échap : annule la sélection et coupe l'aperçu
/// </summary>
public class Combat_SkillBarUI : MonoBehaviour
{
    [Header("Références")]
    public Entity_StatistiqueCombat ownerStats;   // Entité porteuse du SkillBook
    public Entity_SkillCaster ownerCaster;        // Lanceur (reçoit equippedSkill)
    public Combat_PhaseManager phaseManager;      // Optionnel (auto si null)
    public TileGrid_Manager tileGrid;             // Optionnel (auto si null)

    [Header("UI")]
    public Transform buttonContainer;             // Parent des boutons
    public Button buttonPrefab;                   // Prefab bouton (icône only)

    [Header("Comportement")]
    [Tooltip("Démarrer avec un skill sélectionné (laisser false pour 'aucun skill au début du combat').")]
    public bool autoSelectFirstOnEnable = false;

    [Tooltip("Échap = annule la sélection et coupe l'aperçu.")]
    public bool cancelOnEscapeKey = true;

    [Tooltip("Verrouiller l'UI quand ce n'est pas le tour du propriétaire.")]
    public bool lockUiWhenNotMyTurn = true;

    // Boutons instanciés
    private readonly List<Button> spawnedButtons = new List<Button>();

    // Sélection courante
    private Skill_Binding selectedBinding = null;
    private Data_Skill selectedSkill = null;

    // État d’aperçu de zone
    private GameObject lastTargetTile = null;
    private readonly List<GameObject> lastPreviewTiles = new();

    // Verrouillage UI
    private CanvasGroup rootGroup;     // CanvasGroup pour bloquer clicks + raycasts
    private bool wasMyTurn = false;    // Détection de changement de tour


    // --- Awake -------------------------------------------------------------
    // Auto-récupère les références manquantes et garantit un CanvasGroup.
    private void Awake()
    {
        if (!ownerStats) ownerStats = GetComponentInParent<Entity_StatistiqueCombat>();
        if (!ownerCaster) ownerCaster = GetComponentInParent<Entity_SkillCaster>();
        if (!buttonContainer) buttonContainer = transform;

        if (!phaseManager) phaseManager = FindAnyObjectByType<Combat_PhaseManager>();
        if (!tileGrid && phaseManager) tileGrid = phaseManager.tileGrid;

        rootGroup = GetComponent<CanvasGroup>();
        if (!rootGroup) rootGroup = gameObject.AddComponent<CanvasGroup>(); // garanti le verrou
    }

    // --- OnEnable ----------------------------------------------------------
    // Construit la barre et démarre sans sélection (sauf si autoSelectFirstOnEnable == true).
    private void OnEnable()
    {
        BuildFromOwner();
        ClearSkillPreview(); // coupe tout reste visuel
        if (!autoSelectFirstOnEnable) ClearSelectedSkill(); // aucun sort sélectionné au début

        // Mise à l'état cohérent selon le tour courant
        UpdateUiLockByTurn(forceRefresh: true);
    }

    // --- OnDisable ---------------------------------------------------------
    // Nettoie proprement en cas de masquage/désactivation de l’UI.
    private void OnDisable()
    {
        ClearSkillPreview();
        ClearSelectedSkill();
    }

    // --- Update ------------------------------------------------------------
    // Gère l’aperçu, le verrou hors-tour et le nettoyage après clic droit.
    private void Update()
    {
        // 1) Verrouillage par tour
        if (lockUiWhenNotMyTurn)
        {
            if (!UpdateUiLockByTurn())
                return; // hors-tour → on stoppe toute logique (pas de preview, pas de clic)
        }

        // 2) Échap : annule la sélection + coupe l’aperçu
        if (cancelOnEscapeKey && Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSkillPreview();
            ClearSelectedSkill();
            return;
        }

        // 3) Sans compétence sélectionnée -> pas d’aperçu
        if (!selectedSkill || tileGrid == null) return;

        // 4) Tuile sous la souris (raycast)
        GameObject underCursor = GetTileUnderCursor();

        // 5) Clic droit : on laisse Entity_SkillCaster faire le cast,
        //    puis on nettoie au frame suivant pour éviter un double-cast.
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ClearSelectionNextFrame());
            return;
        }

        // 6) Si on sort de la grille → nettoyer l’aperçu
        if (underCursor == null)
        {
            if (lastPreviewTiles.Count > 0) ClearPreviewNow();
            lastTargetTile = null;
            return;
        }

        // 7) Recalculer l’aperçu uniquement si la tuile ciblée change
        if (underCursor != lastTargetTile)
        {
            ShowImpactZoneForTarget(underCursor, selectedSkill);
            lastTargetTile = underCursor;
        }
    }

    // =====================================================================
    // ========================  Construction UI  ===========================
    // =====================================================================

    /// <summary>
    /// Instancie les boutons depuis le SkillBook et branche la sélection de skill.
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

            // Icône only
            var img = btn.GetComponent<Image>();
            if (img)
            {
                img.sprite = data.icon;
                img.preserveAspect = true;
            }

            // Cache tout texte éventuel
            var legacyText = btn.GetComponentInChildren<Text>(true);
            if (legacyText) legacyText.gameObject.SetActive(false);
            var tmpText = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmpText) tmpText.gameObject.SetActive(false);

            // Capture locale
            Skill_Binding captured = binding;

            // Clic sur le bouton = sélectionner le skill + activer la prévisualisation
            btn.onClick.AddListener(() =>
            {
                // Équipe la compétence (le caster gère le clic droit dans son Update).
                ownerCaster.equippedSkill = captured.skill;

                // Mémorise la sélection pour l’aperçu
                selectedBinding = captured;
                selectedSkill = captured.skill;

                // Feedback visuel
                Highlight(btn);

                // Force un recalcul d’aperçu au prochain Update
                lastTargetTile = null;
            });
        }

        // Optionnel : auto-sélection (laisser OFF pour "aucun sort au début")
        if (autoSelectFirstOnEnable && spawnedButtons.Count > 0)
            spawnedButtons[0].onClick.Invoke();
    }

    /// <summary> Détruit tous les boutons instanciés. </summary>
    public void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i])
                Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }

    /// <summary> Met en évidence un bouton actif. </summary>
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

    /// <summary> Réinitialise l’apparence de tous les boutons (aucun actif). </summary>
    private void ResetHighlight()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            var b = spawnedButtons[i];
            if (!b) continue;

            var colors = b.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            b.colors = colors;
        }
    }

    // =====================================================================
    // =====================  LOGIQUE APERÇU DE ZONE  ======================
    // =====================================================================

    /// <summary> Coupe l’aperçu (sans toucher à la sélection). </summary>
    public void ClearSkillPreview()
    {
        lastTargetTile = null;
        ClearPreviewNow();
    }

    /// <summary> Désélectionne complètement le skill (UI + caster + état interne). </summary>
    private void ClearSelectedSkill()
    {
        selectedSkill = null;
        selectedBinding = null;

        // Important : on enlève la compétence du caster pour éviter qu’elle reste en mémoire.
        if (ownerCaster) ownerCaster.equippedSkill = null;

        // Reset visuel des boutons
        ResetHighlight();
    }

    // Nettoyage différé d’un frame pour laisser Entity_SkillCaster capter le clic droit.
    private System.Collections.IEnumerator ClearSelectionNextFrame()
    {
        yield return null; // attendre la fin du frame courant
        ClearSkillPreview();
        ClearSelectedSkill();
    }

    // Raycast caméra → tuile (attend un collider sur la tuile/parent)
    private GameObject GetTileUnderCursor()
    {
        if (!Camera.main) return null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return null;

        var go = hit.collider.gameObject;
        if (go.TryGetComponent(out SetupTile _)) return go;

        var parent = go.GetComponentInParent<SetupTile>();
        return parent ? parent.gameObject : null;
    }

    // Calcule la zone (offsets relatifs) autour de la tuile ciblée et allume MatZoneImpact.
    private void ShowImpactZoneForTarget(GameObject targetTile, Data_Skill skill)
    {
        if (targetTile == null || skill == null || tileGrid == null) { ClearPreviewNow(); return; }
        if (!targetTile.TryGetComponent(out SetupTile targetSetup)) { ClearPreviewNow(); return; }

        Vector2Int center = new Vector2Int(targetSetup.tileX, targetSetup.tileY);

        // Offsets relatifs depuis Data_Skill.impactZone (lecture robuste, incl. "zone")
        List<Vector2Int> offsets = GetOffsetsFromImpactZone(skill);
        if (offsets == null || offsets.Count == 0)
            offsets = new List<Vector2Int> { Vector2Int.zero };

        var newPreview = new List<GameObject>();

        for (int i = 0; i < offsets.Count; i++)
        {
            Vector2Int c = center + offsets[i];
            GameObject tileObj = tileGrid.GetTileAtCoordinates(c.x, c.y);
            if (!tileObj) continue;

            newPreview.Add(tileObj);

            if (tileObj.TryGetComponent(out Tile_Visual visual))
                visual.SetImpactPreview(true); // MatZoneImpact ON
        }

        // Éteint les anciennes tuiles non concernées
        for (int i = 0; i < lastPreviewTiles.Count; i++)
        {
            var t = lastPreviewTiles[i];
            if (!t) continue;
            if (newPreview.Contains(t)) continue;

            if (t.TryGetComponent(out Tile_Visual oldVisual))
                oldVisual.SetImpactPreview(false);
        }

        lastPreviewTiles.Clear();
        lastPreviewTiles.AddRange(newPreview);
    }

    // Coupe immédiatement MatZoneImpact partout.
    private void ClearPreviewNow()
    {
        for (int i = 0; i < lastPreviewTiles.Count; i++)
        {
            var t = lastPreviewTiles[i];
            if (!t) continue;
            if (t.TryGetComponent(out Tile_Visual visual))
                visual.SetImpactPreview(false);
        }
        lastPreviewTiles.Clear();
    }

    // Lecture robuste des offsets relatifs depuis skill.impactZone.
    // Cherche (sans casse) une méthode/propriété/champ parmi : zone / Offsets / RelativeOffsets / Cells / Pattern.
    private List<Vector2Int> GetOffsetsFromImpactZone(Data_Skill skill)
    {
        if (skill == null || skill.impactZone == null) return null;

        object zone = skill.impactZone;
        System.Type t = zone.GetType();
        const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

        // Méthode GetRelativeOffsets()
        var m = t.GetMethod("GetRelativeOffsets", BF, null, System.Type.EmptyTypes, null);
        if (m != null)
        {
            object r = m.Invoke(zone, null);
            if (r is List<Vector2Int> list1) return list1;
            if (r is Vector2Int[] arr1) return new List<Vector2Int>(arr1);
        }

        // Propriétés possibles (incluant "zone")
        string[] propNames = { "zone", "Zone", "Offsets", "RelativeOffsets", "Cells", "Pattern" };
        for (int i = 0; i < propNames.Length; i++)
        {
            var p = t.GetProperty(propNames[i], BF);
            if (p == null) continue;
            object v = p.GetValue(zone);
            if (v is List<Vector2Int> list2) return list2;
            if (v is Vector2Int[] arr2) return new List<Vector2Int>(arr2);
        }

        // Champs possibles (incluant "zone")
        string[] fieldNames = { "zone", "Zone", "offsets", "relativeOffsets", "cells", "pattern" };
        for (int i = 0; i < fieldNames.Length; i++)
        {
            var f = t.GetField(fieldNames[i], BF);
            if (f == null) continue;
            object v = f.GetValue(zone);
            if (v is List<Vector2Int> list3) return list3;
            if (v is Vector2Int[] arr3) return new List<Vector2Int>(arr3);
        }

        return null; // rien trouvé
    }

    // =====================================================================
    // =======================  Verrouillage par tour  ======================
    // =====================================================================

    /// <summary>
    /// Met à jour le verrou UI selon si c'est le tour de ownerStats. 
    /// Retourne true si c'est le tour (UI active), false sinon (UI bloquée).
    /// </summary>
    private bool UpdateUiLockByTurn(bool forceRefresh = false)
    {
        bool isMyTurn = false;
        if (phaseManager && phaseManager.phaseTurn)
        {
            GameObject who = ownerStats ? ownerStats.gameObject : (ownerCaster ? ownerCaster.gameObject : null);
            isMyTurn = phaseManager.phaseTurn.IsMyTurn(who);
        }

        // Applique le lock si demandé
        if (lockUiWhenNotMyTurn)
            SetUiLocked(!isMyTurn);

        // Si on vient de perdre le tour → couper la preview (évite feedback trompeur)
        if ((forceRefresh || wasMyTurn != isMyTurn) && !isMyTurn)
            ClearSkillPreview();

        wasMyTurn = isMyTurn;

        // Hors-tour = renvoyer false pour court-circuiter l'Update (pas de preview/inputs)
        return isMyTurn;
    }

    /// <summary>
    /// Active/désactive l’interactivité de la SkillBar (bloque clicks + raycasts, feedback alpha).
    /// </summary>
    private void SetUiLocked(bool locked)
    {
        if (!rootGroup) return;
        rootGroup.interactable = !locked;
        rootGroup.blocksRaycasts = !locked;
        rootGroup.alpha = locked ? 0.6f : 1f;
    }
}
