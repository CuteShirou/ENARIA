using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Combat/UI/Timeline Combat UI")]
public class Timeline_CombatUI : MonoBehaviour
{
    [Header("Scroll View refs")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;

    [Header("Prefabs (attente / actif)")]
    [SerializeField] private GameObject prefabWaiting; // Profil_Entity
    [SerializeField] private GameObject prefabActive;  // Profil_Entity_Actif

    [Header("Référence du panel d'info (Drag & Drop)")]
    [SerializeField] private InfoEntityPanelUI infoPanel;
    public InfoEntityPanelUI InfoPanel => infoPanel;

    private Combat_PhaseManager manager;

    private class Row
    {
        public GameObject entity;
        public GameObject go;
        public Timeline_ProfilUI ui;
        public bool isActive;
    }
    private readonly List<Row> rows = new();
    private int currentIndex = -1; // -1 = aucun actif

    // (Re)construit TOUTE la timeline, sans actif
    public void BuildFromManager(Combat_PhaseManager mng)
    {
        manager = mng;
        ClearInternal();

        var fighters = manager?.phaseEnter?.AllFighters;
        if (fighters == null) return;

        foreach (var e in fighters)
        {
            if (!e) continue;
            rows.Add(CreateRow(e, false)); // attente par défaut
        }

        currentIndex = -1;
        SnapToIndex(0);
    }

    public void SetNoActive()
    {
        if (rows.Count == 0) return;
        for (int i = 0; i < rows.Count; i++)
            if (rows[i].isActive) SwapRowPrefab(i, wantActive: false);
        currentIndex = -1;
        SnapToIndex(0);
    }

    public void SetCurrentEntity(GameObject entity)
    {
        int idx = rows.FindIndex(r => r.entity == entity);
        SetCurrentIndex(idx);
    }

    public GameObject GetCurrentEntity()
    {
        if (currentIndex < 0 || currentIndex >= rows.Count) return null;
        return rows[currentIndex].entity;
    }

    public GameObject NextActive()
    {
        if (rows.Count == 0) return null;
        int next = (currentIndex < 0) ? 0 : (currentIndex + 1) % rows.Count;
        SetCurrentIndex(next);
        return rows[currentIndex].entity;
    }

    public void RefreshAllHP()
    {
        foreach (var r in rows) r.ui?.RefreshHP();
    }

    public void ClearTimeline()
    {
        foreach (Transform c in content) Destroy(c.gameObject);
        rows.Clear();
        currentIndex = -1;
        if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
    }

    // ---------- internals ----------
    private Row CreateRow(GameObject entity, bool isActive)
    {
        var prefab = isActive ? prefabActive : prefabWaiting;
        var go = Instantiate(prefab, content);

        var ui = go.GetComponent<Timeline_ProfilUI>();
        if (!ui) ui = go.AddComponent<Timeline_ProfilUI>();

        // >>> pas de Find : on passe la réf du panel ici
        ui.Bind(entity, infoPanel);

        return new Row { entity = entity, go = go, ui = ui, isActive = isActive };
    }

    private void SetCurrentIndex(int index)
    {
        if (rows.Count == 0) { currentIndex = -1; return; }
        if (index < 0)
        {
            if (currentIndex >= 0 && currentIndex < rows.Count) SwapRowPrefab(currentIndex, wantActive: false);
            currentIndex = -1;
            return;
        }

        index = Mathf.Clamp(index, 0, rows.Count - 1);

        if (currentIndex >= 0 && currentIndex < rows.Count)
            SwapRowPrefab(currentIndex, wantActive: false);

        SwapRowPrefab(index, wantActive: true);
        currentIndex = index;
        SnapToIndex(index);
    }

    private void SwapRowPrefab(int rowIndex, bool wantActive)
    {
        var old = rows[rowIndex];
        if (old.isActive == wantActive) return;

        var parent = old.go.transform.parent;
        int sibling = old.go.transform.GetSiblingIndex();
        Destroy(old.go);

        var prefab = wantActive ? prefabActive : prefabWaiting;
        var go = Instantiate(prefab, parent);
        go.transform.SetSiblingIndex(sibling);

        var ui = go.GetComponent<Timeline_ProfilUI>();
        if (!ui) ui = go.AddComponent<Timeline_ProfilUI>();
        ui.Bind(old.entity, infoPanel); // <<< panel transmis

        rows[rowIndex].go = go;
        rows[rowIndex].ui = ui;
        rows[rowIndex].isActive = wantActive;
    }

    private void SnapToIndex(int index)
    {
        if (!scrollRect || rows.Count == 0) return;
        float t = Mathf.Clamp01(rows.Count == 1 ? 0f : (float)index / (rows.Count - 1));
        scrollRect.verticalNormalizedPosition = 1f - t;
    }

    private void ClearInternal()
    {
        foreach (Transform c in content) Destroy(c.gameObject);
        rows.Clear();
        currentIndex = -1;
    }
}
