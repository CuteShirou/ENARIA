using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_MiniInfo_Bubble : MonoBehaviour
{
    [Header("Root")]
    public Canvas rootCanvas;                // Canvas racine (auto si laissé vide)
    public RectTransform panel;              // RectTransform du panneau

    [Header("Texts")]
    public TMP_Text valuePA;
    public TMP_Text valuePO;
    public TMP_Text valueDegat;
    public TMP_Text valueCooldown;
    public TMP_Text valueMaxForTarget;

    [Header("Icon")]
    public Image iconSkill;

    [Header("Impact Grid")]
    public RectTransform gridRoot;           // Conteneur UI pour la grille
    public Image cellPrefab;                 // DOIT être un UI Image (ex: "CellTemplate" désactivé)
    public int gridSize = 11;                // Impair recommandé (centre au milieu)
    public int cellSize = 16;                // Taille visuelle (px) d'une case
    public Vector2 cellSpacing = new Vector2(1f, 1f); // Espace entre cases (px)
    public bool autoResizeGridRoot = true;   // Ajuste automatiquement la taille du gridRoot
    public bool followMouse = true;          // Suit la souris si true

    [Header("Mouse Offset")]
    public Vector2 mouseOffset = new Vector2(24f, 120f); // Décalage par rapport au curseur (Y positif = vers le haut)

    [Header("Colors")]
    public Color colorDefault = new Color(0.2f, 0.2f, 0.2f, 1f);  // Couleur des cases vides
    public Color colorCenter = Color.red;                         // Couleur de la case (0,0)
    public Color colorImpact = new Color(0.9f, 0.3f, 0.3f, 1f);   // Couleur des cases impactées
    public Color gridLineColor = new Color(0f, 0f, 0f, 0.45f);     // Couleur « lignes de grille » (fond du GridRoot)

    // Internes
    private readonly List<Image> cells = new List<Image>();
    private bool isVisible = false;

    private int lastBuiltGridSize = -1;
    private int lastBuiltCellSize = -1;
    private Vector2 lastBuiltSpacing = new Vector2(-1, -1);
    private int lastPrefabID = -1;

    void Awake()
    {
        // Préparation initiale
        if (!panel) panel = (RectTransform)transform;
        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();

        if (gridRoot && cellPrefab) BuildGrid();
        Hide();
    }

    void Update()
    {
        // Déplacement de la pop-up si visible
        if (isVisible && followMouse) PlaceAt(Input.mousePosition);
    }

    // Affiche la pop-up avec les infos du skill
    public void Show(Data_Skill skill, Vector2 screenPos)
    {
        if (!skill) return;

        EnsureGridReady();
        FillFromSkill(skill);

        gameObject.SetActive(true);
        isVisible = true;
        PlaceAt(screenPos);
    }

    // Masque la pop-up
    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
    }

    // Renseigne les textes et l'icône
    public void FillFromSkill(Data_Skill skill)
    {
        if (valuePA) valuePA.text = skill.costPA.ToString();
        if (valuePO) valuePO.text = $"{skill.rangeMin} ~ {skill.rangeMax}";
        if (valueDegat)
        {
            if (skill.skillType == SkillType.Attack)
                valueDegat.text = $"{skill.damageMin} ~ {skill.damageMax} {GetElementLabel(skill.skillElement)}";
            else
                valueDegat.text = "-";
        }
        if (valueCooldown) valueCooldown.text = skill.cooldown.ToString();
        if (valueMaxForTarget) valueMaxForTarget.text = skill.maxPerTargetPerTurn.ToString();
        if (iconSkill) iconSkill.sprite = skill.icon;

        DrawImpactZone(skill);
    }

    // Assure que la grille correspond aux paramètres (taille, espacements, prefab)
    void EnsureGridReady()
    {
        if (!gridRoot || !cellPrefab) return;

        int prefabId = cellPrefab ? cellPrefab.GetInstanceID() : -1;
        bool needRebuild =
            cells.Count != gridSize * gridSize ||
            lastBuiltGridSize != gridSize ||
            lastBuiltCellSize != cellSize ||
            lastPrefabID != prefabId ||
            lastBuiltSpacing != cellSpacing;

        if (needRebuild) BuildGrid();
    }

    // Construit (ou reconstruit) la grille
    void BuildGrid()
    {
        if (!gridRoot || !cellPrefab) return;

        // Nettoyage
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            Transform c = gridRoot.GetChild(i);
            if (c != cellPrefab.transform) DestroyImmediate(c.gameObject);
        }
        cells.Clear();

        // Couleur de fond pour que l'espacement devienne des « lignes »
        var bg = gridRoot.GetComponent<Image>();
        if (!bg) bg = gridRoot.gameObject.AddComponent<Image>();
        bg.raycastTarget = false;
        bg.color = gridLineColor;

        // Layout
        var gl = gridRoot.GetComponent<GridLayoutGroup>();
        if (!gl) gl = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = gridSize;
        gl.cellSize = new Vector2(cellSize, cellSize);
        gl.spacing = cellSpacing;
        gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gl.startAxis = GridLayoutGroup.Axis.Horizontal;

        // Ajuste la taille du gridRoot pour inclure l'espacement
        if (autoResizeGridRoot)
        {
            float totalW = gridSize * cellSize + Mathf.Max(0, gridSize - 1) * cellSpacing.x;
            float totalH = gridSize * cellSize + Mathf.Max(0, gridSize - 1) * cellSpacing.y;
            gridRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalW);
            gridRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalH);
        }

        // Instanciation des cases
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            Image img = Instantiate(cellPrefab, gridRoot);
            img.gameObject.SetActive(true);
            img.raycastTarget = false;
            cells.Add(img);
        }

        // Le template reste caché
        cellPrefab.gameObject.SetActive(false);

        lastBuiltGridSize = gridSize;
        lastBuiltCellSize = cellSize;
        lastPrefabID = cellPrefab ? cellPrefab.GetInstanceID() : -1;
        lastBuiltSpacing = cellSpacing;
    }

    // Colore la zone d'impact
    void DrawImpactZone(Data_Skill skill)
    {
        if (cells.Count == 0) return;

        for (int i = 0; i < cells.Count; i++)
            cells[i].color = colorDefault;

        int cx = gridSize / 2;
        int cy = cx;

        int centerIndex = cy * gridSize + cx;
        if (centerIndex >= 0 && centerIndex < cells.Count)
            cells[centerIndex].color = colorCenter;

        List<Vector2Int> offsets = ExtractOffsets(skill);
        if (offsets == null || offsets.Count == 0) return;

        for (int i = 0; i < offsets.Count; i++)
        {
            Vector2Int o = offsets[i];
            int x = cx + o.x;
            int y = cy + o.y;
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) continue;
            int idx = y * gridSize + x;
            cells[idx].color = colorImpact;
        }
    }

    // Récupère les offsets relatifs (Vector2Int[]) depuis la data
    List<Vector2Int> ExtractOffsets(Data_Skill skill)
    {
        if (skill == null || skill.impactZone == null || skill.impactZone.zone == null)
            return null;
        return new List<Vector2Int>(skill.impactZone.zone);
    }

    // Positionne la pop-up près de la souris, avec décalage
    void PlaceAt(Vector2 screenPos)
    {
        if (!rootCanvas) return;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Vector2 localPoint;
        Camera cam = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out localPoint);

        Vector2 pos = localPoint + mouseOffset;

        Rect cr = canvasRect.rect;
        Vector2 half = panel.rect.size * 0.5f;
        pos.x = Mathf.Clamp(pos.x, cr.xMin + half.x, cr.xMax - half.x);
        pos.y = Mathf.Clamp(pos.y, cr.yMin + half.y, cr.yMax - half.y);

        panel.anchoredPosition = pos;
    }

    // Libellés éléments pour l'UI
    string GetElementLabel(SkillElement e)
    {
        switch (e)
        {
            case SkillElement.Force: return "Force";
            case SkillElement.Dexterité: return "Dextérité";
            case SkillElement.Magie: return "Magie";
            case SkillElement.Foi: return "Foi";
            default: return "";
        }
    }
}
