using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tool_CreateSkill : fenêtre d’édition pour créer des assets Data_Skill
/// </summary>
public class Tool_CreateSkill : EditorWindow
{
    // ---------- State général ----------
    private Vector2 scrollPos;                 //   Scroll principal de la fenêtre
    private const int gridSize = 11;           //   Taille de la grille (11x11)
    private const int cellSize = 20;           //   Taille visuelle de chaque case
    private Vector2Int center => new Vector2Int(gridSize / 2, gridSize / 2); //   Centre (0,0)

    // ---------- Données temporaires de saisie ----------
    private int id = 0;                        //   ID unique
    private string skillName = "NewSkill";     //   Nom de la compétence
    private string description = "";           //   Description

    private SkillType skillType = SkillType.Attack;       //   Type
    private SkillElement skillElement = SkillElement.None; //   Élément

    private int damageMin = 0;                 //   Dégâts min
    private int damageMax = 0;                 //   Dégâts max
    private int costPA = 3;                    //   Coût PA
    private int rangeMin = 1;                  //   Portée min
    private int rangeMax = 5;                  //   Portée max
    private int cooldown = 0;                  //   Cooldown (tours)
    private int maxPerTargetPerTurn = 99;      //   Lancers max par cible/tour

    private float critChance = 0f;             //   % critique
    private Sprite icon = null;                //   Icône

    //   Effets normaux / critiques (édition dans l’outil)
    private List<SkillEffect> effects = new List<SkillEffect>();
    private List<SkillEffect> critEffects = new List<SkillEffect>();

    //   Grille de zone d’impact (sélection utilisateur)
    private bool[,] gridSelection = new bool[gridSize, gridSize];

    // ====== Menu ======
    [MenuItem("Window/Skill Creator")]
    public static void ShowWindow()
    {
        //   Ouvre/affiche la fenêtre d’édition
        GetWindow<Tool_CreateSkill>("Skill Creator");
    }

    // ====== Cycle Unity Editor ======
    private void OnEnable()
    {
        //   Initialisation quand la fenêtre s’ouvre
        ResetGrid();
    }

    private void OnGUI()
    {
        //   UI principale : configuration + grille + sauvegarde
        EditorGUILayout.LabelField("Configuration de la compétence", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawIdentitySection(); EditorGUILayout.Space(10);
        DrawClassificationSection(); EditorGUILayout.Space(10);
        DrawStatsSection(); EditorGUILayout.Space(10);
        DrawEffectsSection(); EditorGUILayout.Space(10);
        DrawIconSection(); EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Zone d'Impact (relative à la cible)", EditorStyles.boldLabel);
        DrawGrid(); EditorGUILayout.Space(10);

        if (GUILayout.Button("Réinitialiser la grille"))
        {
            //   Remet toutes les cases à faux
            ResetGrid();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(10);

        if (GUILayout.Button("Sauvegarder la compétence"))
        {
            SaveSkillAsset();
        }
    }

    // ====== Sections UI ======
    private void DrawIdentitySection()
    {
        //   Saisie des infos d’identification de la compétence
        id = EditorGUILayout.IntField("ID", id);
        skillName = EditorGUILayout.TextField("Nom", skillName);
        EditorGUILayout.LabelField("Description");
        description = EditorGUILayout.TextArea(description, GUILayout.MinHeight(40));
    }

    private void DrawClassificationSection()
    {
        //   Sélection du type et de l’élément de la compétence
        skillType = (SkillType)EditorGUILayout.EnumPopup("Type", skillType);
        skillElement = (SkillElement)EditorGUILayout.EnumPopup("Élément", skillElement);
    }

    private void DrawStatsSection()
    {
        //   Statistiques numériques principales (+ garde-fous simples)
        EditorGUILayout.LabelField("Statistiques", EditorStyles.boldLabel);
        damageMin = Mathf.Max(0, EditorGUILayout.IntField("Dégâts min", damageMin));
        damageMax = Mathf.Max(damageMin, EditorGUILayout.IntField("Dégâts max", damageMax));
        costPA = Mathf.Max(0, EditorGUILayout.IntField("Coût PA", costPA));
        rangeMin = Mathf.Max(0, EditorGUILayout.IntField("Portée min", rangeMin));
        rangeMax = Mathf.Max(rangeMin, EditorGUILayout.IntField("Portée max", rangeMax));
        cooldown = Mathf.Max(0, EditorGUILayout.IntField("Cooldown (tours)", cooldown));
        maxPerTargetPerTurn = Mathf.Max(0, EditorGUILayout.IntField("Max par cible par tour", maxPerTargetPerTurn));
    }

    private void DrawEffectsSection()
    {
        //   Effets non-critiques
        EditorGUILayout.LabelField("Effets (non-critique)", EditorStyles.boldLabel);
        DrawEffectList(effects);

        EditorGUILayout.Space(6);

        //   Chance de critique + Effets critiques
        critChance = EditorGUILayout.Slider("Chance critique (%)", critChance, 0f, 100f);
        EditorGUILayout.LabelField("Effets bonus si critique", EditorStyles.boldLabel);
        DrawEffectList(critEffects);
    }

    private void DrawIconSection()
    {
        //   Sélection de l’icône de la compétence
        icon = (Sprite)EditorGUILayout.ObjectField("Icône", icon, typeof(Sprite), false);
    }

    private void DrawEffectList(List<SkillEffect> list)
    {
        //   Petite UI pour ajouter/éditer/supprimer des effets
        int removeIndex = -1;

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            if (list[i] == null) list[i] = new SkillEffect();

            list[i].effectType = (EffectType)EditorGUILayout.EnumPopup("Type d'effet", list[i].effectType);
            list[i].value = EditorGUILayout.FloatField("Valeur", list[i].value);
            list[i].duration = EditorGUILayout.IntField("Durée (tours)", list[i].duration);
            list[i].applyToSelf = EditorGUILayout.Toggle("S'applique à soi", list[i].applyToSelf);

            if (GUILayout.Button("Supprimer cet effet"))
                removeIndex = i;

            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0 && removeIndex < list.Count)
            list.RemoveAt(removeIndex);

        if (GUILayout.Button("Ajouter un effet"))
            list.Add(new SkillEffect());
    }

    // ====== Grille d’impact ======
    private void DrawGrid()
    {
        //   Dessine une grille 11x11 de toggles ; centre en rouge
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

    private void ResetGrid()
    {
        //   Réinitialise la grille (toutes les cases à faux)
        gridSelection = new bool[gridSize, gridSize];
    }

    // ====== Sauvegarde ======
    private void SaveSkillAsset()
    {
        //   Construit un Data_Skill et enregistre un asset
        var newSkill = CreateInstance<Data_Skill>();

        // -- Remplissage des champs --
        newSkill.ID = id;
        newSkill.skillName = skillName;
        newSkill.description = description;

        newSkill.skillType = skillType;
        newSkill.skillElement = skillElement;

        newSkill.damageMin = damageMin;
        newSkill.damageMax = damageMax;
        newSkill.costPA = costPA;
        newSkill.rangeMin = rangeMin;
        newSkill.rangeMax = rangeMax;
        newSkill.cooldown = cooldown;
        newSkill.maxPerTargetPerTurn = maxPerTargetPerTurn;

        newSkill.critChance = critChance;
        newSkill.icon = icon;

        // -- Zone d’impact depuis la grille --
        var positions = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (gridSelection[x, y])
                {
                    //   Coordonnées relatives autour de (0,0)
                    Vector2Int relativePos = new Vector2Int(x, y) - center;
                    positions.Add(relativePos);
                }
            }
        }
        if (newSkill.impactZone == null)
            newSkill.impactZone = new ImpactZone();
        newSkill.impactZone.zone = positions.ToArray(); //   tableau, comme dans SkillType.cs

        // -- Effets --
        newSkill.effects = new List<SkillEffect>(effects);
        newSkill.critEffects = new List<SkillEffect>(critEffects);

        // -- Dossier cible Resources/List_Skill --
        string folderPath = "Assets/Resources/List_Skill";
        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);

        // -- Nom d’asset basé sur le nom de la compétence --
        string safeName = string.IsNullOrWhiteSpace(skillName) ? "Skill" : skillName.Trim();
        string assetPath = $"{folderPath}/{safeName}.asset";

        AssetDatabase.CreateAsset(newSkill, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Compétence sauvegardée", $"Compétence '{safeName}' enregistrée avec succès !", "OK");
    }
}
