using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CombatMapEditorWindow : EditorWindow
{
    private string mapName = "NewMap";
    private int width = 10;
    private int height = 10;

    private enum CellType { None, Green, Red, Blue }
    private CellType currentPaint = CellType.Green;

    private Dictionary<Vector2Int, CellType> cellMap = new();
    private Vector2 scrollPos;

    [MenuItem("Window/Combat Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<CombatMapEditorWindow>("Combat Map Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Configuration de la carte", EditorStyles.boldLabel);
        mapName = EditorGUILayout.TextField("Nom de la carte", mapName);
        width = EditorGUILayout.IntField("Largeur", width);
        height = EditorGUILayout.IntField("Hauteur", height);

        if (GUILayout.Button("Créer la grille"))
        {
            cellMap.Clear();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    cellMap[new Vector2Int(x, y)] = CellType.None;
        }

        EditorGUILayout.Space(10);
        currentPaint = (CellType)EditorGUILayout.EnumPopup("Mode de dessin", currentPaint);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Sauvegarder la carte"))
        {
            SaveMap();
        }
    }

    private void DrawGrid()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new(x, y);
                CellType type = cellMap.ContainsKey(pos) ? cellMap[pos] : CellType.None;

                Color color = Color.gray;
                switch (type)
                {
                    case CellType.Green: color = Color.green; break;
                    case CellType.Red: color = Color.red; break;
                    case CellType.Blue: color = Color.cyan; break;
                }

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = color;

                if (GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (type == currentPaint)
                        cellMap[pos] = CellType.None;
                    else
                        cellMap[pos] = currentPaint;
                }

                GUI.backgroundColor = oldColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SaveMap()
    {
        var newMap = CreateInstance<CombatMapData>();
        newMap.mapName = mapName;
        newMap.width = width;
        newMap.height = height;

        newMap.greenTeamPositions.Clear();
        newMap.redTeamPositions.Clear();
        newMap.interactiveObjectPositions.Clear();

        foreach (var cell in cellMap)
        {
            switch (cell.Value)
            {
                case CellType.Green: newMap.greenTeamPositions.Add(cell.Key); break;
                case CellType.Red: newMap.redTeamPositions.Add(cell.Key); break;
                case CellType.Blue: newMap.interactiveObjectPositions.Add(cell.Key); break;
            }
        }

        string folderPath = "Assets/2 - Props/CombatMaps/Maps";
        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);

        string assetPath = $"{folderPath}/{mapName}.asset";
        AssetDatabase.CreateAsset(newMap, assetPath);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Carte sauvegardée", $"Carte {mapName} enregistrée avec succès !", "OK");
    }
}
