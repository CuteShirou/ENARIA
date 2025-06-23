using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillData))]
public class SkillEditor : Editor
{
    private const int gridSize = 11;
    private const int cellSize = 20;
    private Vector2Int center => new Vector2Int(gridSize / 2, gridSize / 2);

    private bool[,] gridSelection = new bool[gridSize, gridSize];

    void OnEnable()
    {
        SkillData skill = (SkillData)target;
        ResetGrid();

        if (skill.impactZone != null && skill.impactZone.zone != null)
        {
            foreach (var pos in skill.impactZone.zone)
            {
                Vector2Int gridPos = pos + center;
                if (IsInGrid(gridPos))
                    gridSelection[gridPos.x, gridPos.y] = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Zone d'Impact (relative à la cible)", EditorStyles.boldLabel);

        DrawGrid();

        if (GUILayout.Button("Appliquer la zone à la compétence"))
        {
            ApplyGridToSkill();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    void DrawGrid()
    {
        for (int y = 0; y < gridSize; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize; x++)
            {
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = (x == center.x && y == center.y) ? Color.red : originalColor;

                bool selected = gridSelection[x, y];
                bool newSelected = GUILayout.Toggle(selected, "", "Button", GUILayout.Width(cellSize), GUILayout.Height(cellSize));
                if (newSelected != selected)
                    gridSelection[x, y] = newSelected;

                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void ApplyGridToSkill()
    {
        SkillData skill = (SkillData)target;
        var positions = new System.Collections.Generic.List<Vector2Int>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (gridSelection[x, y])
                {
                    Vector2Int relativePos = new Vector2Int(x, y) - center;
                    positions.Add(relativePos);
                }
            }
        }

        if (skill.impactZone == null)
            skill.impactZone = new ImpactZone();

        skill.impactZone.zone = positions.ToArray();
    }

    void ResetGrid()
    {
        gridSelection = new bool[gridSize, gridSize];
    }

    bool IsInGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }
}
