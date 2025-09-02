using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Combat_SkillBarUI : MonoBehaviour
{
    [Header("Références")]
    public Entity_StatistiqueCombat ownerStats;
    public Entity_SkillCaster ownerCaster;
    public Combat_PhaseManager phaseManager;
    public TileGrid_Manager tileGrid;

    [Header("UI")]
    public Transform buttonContainer;
    public Button buttonPrefab;

    [Header("Comportement")]
    public bool autoSelectFirstOnEnable = false;
    public bool cancelOnEscapeKey = true;
    public bool lockUiWhenNotMyTurn = true;

    [Header("Mini Info Bubble")]
    public Panel_MiniInfo_Bubble infoBubble; // Pop-up d'information au survol

    private readonly List<Button> spawnedButtons = new List<Button>();

    private Skill_Binding selectedBinding = null;
    private Data_Skill selectedSkill = null;

    private GameObject lastTargetTile = null;
    private readonly List<GameObject> lastPreviewTiles = new();

    private CanvasGroup rootGroup;
    private bool wasMyTurn = false;

    private void Awake()
    {
        if (!ownerStats) ownerStats = GetComponentInParent<Entity_StatistiqueCombat>();
        if (!ownerCaster) ownerCaster = GetComponentInParent<Entity_SkillCaster>();
        if (!buttonContainer) buttonContainer = transform;

        if (!phaseManager) phaseManager = FindAnyObjectByType<Combat_PhaseManager>();
        if (!tileGrid && phaseManager) tileGrid = phaseManager.tileGrid;

        rootGroup = GetComponent<CanvasGroup>();
        if (!rootGroup) rootGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        BuildFromOwner();
        ClearSkillPreview();
        if (!autoSelectFirstOnEnable) ClearSelectedSkill();
        UpdateUiLockByTurn(true);
    }

    private void OnDisable()
    {
        ClearSkillPreview();
        ClearSelectedSkill();
        if (infoBubble) infoBubble.Hide();
    }

    private void Update()
    {
        if (lockUiWhenNotMyTurn)
        {
            if (!UpdateUiLockByTurn())
                return;
        }

        if (cancelOnEscapeKey && Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSkillPreview();
            ClearSelectedSkill();
            if (infoBubble) infoBubble.Hide();
            return;
        }

        if (!selectedSkill || tileGrid == null) return;

        GameObject underCursor = GetTileUnderCursor();

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ClearSelectionNextFrame());
            return;
        }

        if (underCursor == null)
        {
            if (lastPreviewTiles.Count > 0) ClearPreviewNow();
            lastTargetTile = null;
            return;
        }

        if (underCursor != lastTargetTile)
        {
            ShowImpactZoneForTarget(underCursor, selectedSkill);
            lastTargetTile = underCursor;
        }
    }

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

            var img = btn.GetComponent<Image>();
            if (img)
            {
                img.sprite = data.icon;
                img.preserveAspect = true;
            }

            var legacyText = btn.GetComponentInChildren<Text>(true);
            if (legacyText) legacyText.gameObject.SetActive(false);
            var tmpText = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmpText) tmpText.gameObject.SetActive(false);

            // Survol : ajoute le petit composant qui pilotera la pop-up
            var hover = btn.gameObject.AddComponent<SkillButton_Hover>();
            hover.Init(infoBubble, data);

            Skill_Binding captured = binding;

            btn.onClick.AddListener(() =>
            {
                ownerCaster.equippedSkill = captured.skill;

                selectedBinding = captured;
                selectedSkill = captured.skill;

                Highlight(btn);

                lastTargetTile = null;

                if (infoBubble) infoBubble.Hide();
            });
        }

        if (autoSelectFirstOnEnable && spawnedButtons.Count > 0)
            spawnedButtons[0].onClick.Invoke();
    }

    public void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i])
                Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }

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

    public void ClearSkillPreview()
    {
        lastTargetTile = null;
        ClearPreviewNow();
    }

    private void ClearSelectedSkill()
    {
        selectedSkill = null;
        selectedBinding = null;
        if (ownerCaster) ownerCaster.equippedSkill = null;
        ResetHighlight();
    }

    private System.Collections.IEnumerator ClearSelectionNextFrame()
    {
        yield return null;
        ClearSkillPreview();
        ClearSelectedSkill();
        if (infoBubble) infoBubble.Hide();
    }

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

    private void ShowImpactZoneForTarget(GameObject targetTile, Data_Skill skill)
    {
        if (targetTile == null || skill == null || tileGrid == null) { ClearPreviewNow(); return; }
        if (!targetTile.TryGetComponent(out SetupTile targetSetup)) { ClearPreviewNow(); return; }

        Vector2Int center = new Vector2Int(targetSetup.tileX, targetSetup.tileY);

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
                visual.SetImpactPreview(true);
        }

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

    private List<Vector2Int> GetOffsetsFromImpactZone(Data_Skill skill)
    {
        if (skill == null || skill.impactZone == null) return null;

        object zone = skill.impactZone;
        System.Type t = zone.GetType();
        const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

        var m = t.GetMethod("GetRelativeOffsets", BF, null, System.Type.EmptyTypes, null);
        if (m != null)
        {
            object r = m.Invoke(zone, null);
            if (r is List<Vector2Int> list1) return list1;
            if (r is Vector2Int[] arr1) return new List<Vector2Int>(arr1);
        }

        string[] propNames = { "zone", "Zone", "Offsets", "RelativeOffsets", "Cells", "Pattern" };
        for (int i = 0; i < propNames.Length; i++)
        {
            var p = t.GetProperty(propNames[i], BF);
            if (p == null) continue;
            object v = p.GetValue(zone);
            if (v is List<Vector2Int> list2) return list2;
            if (v is Vector2Int[] arr2) return new List<Vector2Int>(arr2);
        }

        string[] fieldNames = { "zone", "Zone", "offsets", "relativeOffsets", "cells", "pattern" };
        for (int i = 0; i < fieldNames.Length; i++)
        {
            var f = t.GetField(fieldNames[i], BF);
            if (f == null) continue;
            object v = f.GetValue(zone);
            if (v is List<Vector2Int> list3) return list3;
            if (v is Vector2Int[] arr3) return new List<Vector2Int>(arr3);
        }

        return null;
    }

    private bool UpdateUiLockByTurn(bool forceRefresh = false)
    {
        bool isMyTurn = false;
        if (phaseManager && phaseManager.phaseTurn)
        {
            GameObject who = ownerStats ? ownerStats.gameObject : (ownerCaster ? ownerCaster.gameObject : null);
            isMyTurn = phaseManager.phaseTurn.IsMyTurn(who);
        }

        if (lockUiWhenNotMyTurn)
            SetUiLocked(!isMyTurn);

        if ((forceRefresh || wasMyTurn != isMyTurn) && !isMyTurn)
        {
            ClearSkillPreview();
            if (infoBubble) infoBubble.Hide();
        }

        wasMyTurn = isMyTurn;
        return isMyTurn;
    }

    private void SetUiLocked(bool locked)
    {
        if (!rootGroup) return;
        rootGroup.interactable = !locked;
        rootGroup.blocksRaycasts = !locked;
        rootGroup.alpha = locked ? 0.6f : 1f;
    }
}
