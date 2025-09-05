// Tool_Animation2D_Creator.cs
// Fenêtre d'édition minimale pour créer un Prefab d'animation 2D (flipbook)
// à partir d'une liste de Sprites. Prévisualisation incluse.
// Menu: Window > Animation2DCreator
// Sauvegarde automatique dans un dossier fixe (pas de popup).
// Ajout: Scroll vertical global pour gérer de longues listes de frames.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;

public class Tool_Animation2D_Creator : EditorWindow
{
    // ============================ Données UI ============================
    // ID technique (ex: "slash_knife")
    private string id = "new_animation";
    // Nom affiché (ex: "Slash Knife")
    private string displayName = "New Animation";
    // Liste de frames (Sprites)
    private List<Sprite> frames = new List<Sprite>();
    // Vitesse d'animation (images/seconde)
    private float framesPerSecond = 12f;

    // Options de lecture
    private bool loop = false;
    private bool playOnAwake = true;
    private bool autoDestroyOnEnd = true;
    private bool useUnscaledTime = false;
    private bool randomStartFrame = false;
    private float startDelay = 0f;

    // Rendu
    private string sortingLayerName = "Default";
    private int sortingOrder = 0;
    private Vector2 prefabScale = Vector2.one;
    private Color tintColor = Color.white;

    // Sortie (cohérence avec tes outils)
    // Dossier où seront stockés les Prefabs d'animation
    private string outputFolder = "Assets/Resources/List_AnimationPrefab";

    // Aperçu
    private bool isPreviewing = false;
    private double previewStartTime = 0.0;
    private int previewFrameIndex = 0;
    private ReorderableList framesList;

    // NOUVEAU: position du scroll global
    private Vector2 scrollPos = Vector2.zero;

    // --------------------------------------------------------------------
    // Ouvre la fenêtre depuis le menu (sous Window pour cohérence)
    // --------------------------------------------------------------------
    [MenuItem("Window/Animation2DCreator")]
    public static void OpenWindow()
    {
        var w = GetWindow<Tool_Animation2D_Creator>("Animation 2D Creator");
        w.minSize = new Vector2(600f, 560f);
        w.Show();
    }

    // --------------------------------------------------------------------
    // Initialisation de la fenêtre
    // --------------------------------------------------------------------
    private void OnEnable()
    {
        // Tick d'éditeur pour animer la prévisualisation
        EditorApplication.update += EditorTick;

        // Liste réordonnable des frames
        framesList = new ReorderableList(frames, typeof(Sprite), true, true, true, true);
        framesList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Frames (Sprites) — Glisser / Réordonner / Supprimer");
        };
        framesList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2f;
            frames[index] = (Sprite)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                $"#{index:00}", frames[index], typeof(Sprite), false);
        };
        framesList.onAddCallback = list => frames.Add(null);
        framesList.onRemoveCallback = list =>
        {
            if (list.index >= 0 && list.index < frames.Count)
                frames.RemoveAt(list.index);
        };
    }

    // --------------------------------------------------------------------
    // Nettoyage
    // --------------------------------------------------------------------
    private void OnDisable()
    {
        EditorApplication.update -= EditorTick;
    }

    // --------------------------------------------------------------------
    // Tick d'éditeur pour l'aperçu
    // --------------------------------------------------------------------
    private void EditorTick()
    {
        // Avance l'aperçu pendant la lecture
        if (isPreviewing && frames != null && frames.Count > 0 && framesPerSecond > 0f)
        {
            double t = EditorApplication.timeSinceStartup - previewStartTime;
            int count = frames.Count;

            if (loop)
            {
                previewFrameIndex = (int)Math.Floor((t * framesPerSecond) % count);
            }
            else
            {
                previewFrameIndex = Mathf.Clamp((int)Math.Floor((float)t * framesPerSecond), 0, count - 1);
                if (previewFrameIndex >= count - 1)
                    isPreviewing = false;
            }
            Repaint();
        }
    }

    // --------------------------------------------------------------------
    // UI
    // --------------------------------------------------------------------
    private void OnGUI()
    {
        // NOUVEAU: Scroll global pour toute la fenêtre
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Metadata", EditorStyles.boldLabel);
        id = EditorGUILayout.TextField("ID", id);
        displayName = EditorGUILayout.TextField("Name", displayName);

        EditorGUILayout.Space(6f);

        GUILayout.Label("Frames", EditorStyles.boldLabel);
        framesList.DoLayoutList();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-Sort By Name"))
        {
            // Trie naturel (0001, 0002, ...)
            frames = frames.Where(s => s != null).OrderBy(s => s.name, new NaturalSortComparer()).ToList();
            framesList.list = frames;
        }
        if (GUILayout.Button("Clear"))
        {
            frames.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6f);

        GUILayout.Label("Playback", EditorStyles.boldLabel);
        framesPerSecond = EditorGUILayout.Slider("Frames Per Second", framesPerSecond, 1f, 60f);
        startDelay = EditorGUILayout.Slider("Start Delay (s)", startDelay, 0f, 2f);
        loop = EditorGUILayout.Toggle("Loop", loop);
        playOnAwake = EditorGUILayout.Toggle("Play On Awake", playOnAwake);
        autoDestroyOnEnd = EditorGUILayout.Toggle("Auto Destroy On End", autoDestroyOnEnd);
        randomStartFrame = EditorGUILayout.Toggle("Random Start Frame", randomStartFrame);
        useUnscaledTime = EditorGUILayout.Toggle("Use Unscaled Time", useUnscaledTime);

        EditorGUILayout.Space(6f);

        GUILayout.Label("Render", EditorStyles.boldLabel);
        sortingLayerName = EditorGUILayout.TextField("Sorting Layer", sortingLayerName);
        sortingOrder = EditorGUILayout.IntField("Order In Layer", sortingOrder);
        prefabScale = EditorGUILayout.Vector2Field("Prefab Scale", prefabScale);
        tintColor = EditorGUILayout.ColorField("Tint Color", tintColor);

        EditorGUILayout.Space(8f);

        // Sortie fixe (pas de popup)
        GUILayout.Label("Output", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        if (GUILayout.Button("Select Folder", GUILayout.MaxWidth(120)))
        {
            string abs = EditorUtility.OpenFolderPanel("Select Output Folder (inside Assets)", Application.dataPath, "");
            if (!string.IsNullOrEmpty(abs))
            {
                if (abs.StartsWith(Application.dataPath))
                {
                    string rel = "Assets" + abs.Substring(Application.dataPath.Length);
                    outputFolder = rel.Replace("\\", "/");
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Le dossier doit se trouver sous 'Assets/'.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox($"Les prefabs seront sauvegardés automatiquement dans :\n{outputFolder}", MessageType.Info);

        EditorGUILayout.Space(8f);

        GUILayout.Label("Preview", EditorStyles.boldLabel);
        DrawPreviewArea();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isPreviewing ? "Stop Preview" : "Play Preview"))
        {
            TogglePreview();
        }
        using (new EditorGUI.DisabledScope(isPreviewing || frames.Count == 0))
        {
            int max = Mathf.Max(0, frames.Count - 1);
            int newIndex = EditorGUILayout.IntSlider("Frame", previewFrameIndex, 0, max);
            if (newIndex != previewFrameIndex)
            {
                previewFrameIndex = newIndex;
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10f);

        using (new EditorGUI.DisabledScope(frames == null || frames.Count == 0))
        {
            if (GUILayout.Button("Create Animation Prefab"))
            {
                CreatePrefab(); // Sauvegarde automatique dans outputFolder
            }
        }

        // FIN du scroll global
        EditorGUILayout.EndScrollView();
    }

    // --------------------------------------------------------------------
    // Aperçu visuel d'une frame
    // --------------------------------------------------------------------
    private void DrawPreviewArea()
    {
        Rect r = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.Height(220f));
        GUI.Box(r, GUIContent.none);

        if (frames == null || frames.Count == 0)
        {
            GUI.Label(r, "Drop Sprites here", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        int idx = Mathf.Clamp(previewFrameIndex, 0, frames.Count - 1);
        Sprite s = frames[idx];
        if (s == null || s.texture == null) return;

        Rect tr = s.textureRect;
        Rect uv = new Rect(tr.x / s.texture.width, tr.y / s.texture.height, tr.width / s.texture.width, tr.height / s.texture.height);

        float spriteRatio = tr.width / tr.height;
        float areaRatio = r.width / r.height;
        Rect drawRect = r;

        if (spriteRatio > areaRatio)
        {
            float h = r.width / spriteRatio;
            drawRect = new Rect(r.x, r.y + (r.height - h) * 0.5f, r.width, h);
        }
        else
        {
            float w = r.height * spriteRatio;
            drawRect = new Rect(r.x + (r.width - w) * 0.5f, r.y, w, r.height);
        }

        GUI.DrawTextureWithTexCoords(drawRect, s.texture, uv);
    }

    // --------------------------------------------------------------------
    // Play/Stop de l'aperçu
    // --------------------------------------------------------------------
    private void TogglePreview()
    {
        if (!isPreviewing)
        {
            previewStartTime = EditorApplication.timeSinceStartup;
            if (randomStartFrame && frames.Count > 0)
                previewFrameIndex = UnityEngine.Random.Range(0, frames.Count);
            isPreviewing = true;
        }
        else
        {
            isPreviewing = false;
        }
        Repaint();
    }

    // --------------------------------------------------------------------
    // Création du Prefab dans le dossier choisi (sans popup)
    // --------------------------------------------------------------------
    private void CreatePrefab()
    {
        // Sécurité sur les frames
        if (frames == null || frames.Count == 0)
        {
            EditorUtility.DisplayDialog("Missing Frames", "Ajoutez au moins 1 Sprite.", "OK");
            return;
        }

        // Sécurité sur le dossier (doit être sous Assets)
        if (string.IsNullOrEmpty(outputFolder) || !outputFolder.StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("Invalid Output Folder", "Le dossier de sortie doit être sous 'Assets/'.", "OK");
            return;
        }

        // S'assure que tous les sous-dossiers existent
        EnsureFolderExists(outputFolder);

        // Construit un nom de fichier propre
        string rawName = $"{id}_{displayName}".Replace(" ", "_");
        string fileName = SanitizeFileName(rawName) + ".prefab";

        // Construit le chemin cible + garantit l'unicité
        string targetPath = $"{outputFolder}/{fileName}";
        targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);

        // Crée un GO temporaire
        GameObject go = new GameObject(displayName);
        try
        {
            // SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = frames[0];
            sr.color = tintColor;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;

            // Échelle par défaut
            go.transform.localScale = new Vector3(prefabScale.x, prefabScale.y, 1f);

            // Lecteur d'animation
            var runner = go.AddComponent<Sprite_AnimationRunner>();
            runner.id = id;
            runner.displayName = displayName;
            runner.frames = frames.ToArray();
            runner.framesPerSecond = framesPerSecond;
            runner.loop = loop;
            runner.playOnAwake = playOnAwake;
            runner.autoDestroyOnEnd = autoDestroyOnEnd;
            runner.useUnscaledTime = useUnscaledTime;
            runner.randomStartFrame = randomStartFrame;
            runner.startDelay = startDelay;

            // Sauvegarde en Prefab
            PrefabUtility.SaveAsPrefabAsset(go, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefab Created", $"Prefab créé dans :\n{targetPath}", "OK");
        }
        finally
        {
            DestroyImmediate(go);
        }
    }

    // --------------------------------------------------------------------
    // Utilitaires : dossier / nom de fichier
    // --------------------------------------------------------------------

    // Crée récursivement les sous-dossiers sous 'Assets' si manquants
    private void EnsureFolderExists(string projectRelativePath)
    {
        // Exemple: "Assets/Resources/List_AnimationPrefab"
        string[] parts = projectRelativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        // Le premier élément doit être "Assets"
        if (parts[0] != "Assets")
            throw new Exception("Le chemin doit commencer par 'Assets'.");

        string currentPath = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = currentPath + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }
            currentPath = nextPath;
        }
    }

    // Remplace les caractères invalides pour un nom de fichier d'asset
    private string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        foreach (char c in invalid)
            name = name.Replace(c.ToString(), "_");
        while (name.Contains("__")) name = name.Replace("__", "_");
        return name.Trim('_');
    }

    // --------------------------------------------------------------------
    // Comparateur de tri "naturel" (A1 < A10 < A100)
    // --------------------------------------------------------------------
    private class NaturalSortComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            int ix = 0, iy = 0;
            while (ix < x.Length && iy < y.Length)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    long vx = 0; while (ix < x.Length && char.IsDigit(x[ix])) vx = vx * 10 + (x[ix++] - '0');
                    long vy = 0; while (iy < y.Length && char.IsDigit(y[iy])) vy = vy * 10 + (y[iy++] - '0');
                    int cmp = vx.CompareTo(vy); if (cmp != 0) return cmp;
                }
                else
                {
                    int cmp = char.ToLowerInvariant(x[ix]).CompareTo(char.ToLowerInvariant(y[iy]));
                    if (cmp != 0) return cmp;
                    ix++; iy++;
                }
            }
            return (x.Length - ix).CompareTo(y.Length - iy);
        }
    }
}
#endif
