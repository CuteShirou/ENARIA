using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Exploration_DisplayInfoGroupMonster : MonoBehaviour
{
    [SerializeField] private TextMeshPro textDisplay;                    // Référence du texte 3D (TMP)
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);     // Décalage monde (X, Y, Z)

    [Header("Materials (survol)")]
    [SerializeField] private Material basicMaterial;                     // Material par défaut (sans contour)
    [SerializeField] private Material displayMaterial;                   // Material au survol (avec contour)
    [SerializeField] private Renderer[] targetRenderers;                 // Renderers à affecter (MeshRenderer, etc.)

    private Exploration_InfoGroupMonster groupTeleporter;

    private void Start()
    {
        // Récupération du contrôleur de groupe sur le parent
        groupTeleporter = GetComponentInParent<Exploration_InfoGroupMonster>();

        // Masquer la pop-up au lancement
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);

        // Appliquer le material de base au démarrage
        ApplyMaterialToTargets(basicMaterial);
    }

    private void OnMouseEnter()
    {
        // Affichage du contenu de la pop-up
        if (textDisplay == null || groupTeleporter == null)
        {
            // Même si pas de texte, on applique quand même le material de survol
            ApplyMaterialToTargets(displayMaterial);
            return;
        }

        List<string> monsterLines = new List<string>();
        foreach (GameObject monster in groupTeleporter.monstersInGroup)
        {
            if (monster == null) continue;

            Entity_Info infos = monster.GetComponent<Entity_Info>();
            if (infos != null)
                monsterLines.Add($"{infos.entity_Name} (LVL {infos.entity_Level})");
            else
                monsterLines.Add($"{monster.name} (LVL ?)");
        }

        textDisplay.text = string.Join("\n", monsterLines);
        textDisplay.gameObject.SetActive(true);

        // Positionner immédiatement la pop-up avec le décalage (X,Y,Z)
        SetPopupPosition();

        // Appliquer le material de survol (avec contour)
        ApplyMaterialToTargets(displayMaterial);
    }

    private void OnMouseExit()
    {
        // Masquer la pop-up
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);

        // Remettre le material de base
        ApplyMaterialToTargets(basicMaterial);
    }

    private void OnDisable()
    {
        // Sécurité si l'objet est désactivé
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);
        ApplyMaterialToTargets(basicMaterial);
    }

    private void Update()
    {
        // Mettre à jour la position (avec offset) si la pop-up est visible
        if (textDisplay != null && textDisplay.gameObject.activeSelf)
        {
            SetPopupPosition();

            // Conserver l'orientation face caméra comme avant
            if (Camera.main != null)
                textDisplay.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // Applique un material à tous les renderers ciblés (couvre tous les sous-meshes)
    private void ApplyMaterialToTargets(Material mat)
    {
        if (mat == null || targetRenderers == null) return;

        for (int r = 0; r < targetRenderers.Length; r++)
        {
            Renderer rend = targetRenderers[r];
            if (rend == null) continue;

            // Remplace tous les sous-meshes par le même material de façon simple
            var mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            rend.materials = mats;
        }
    }

    // Place la pop-up à la position de l'objet + offset (X,Y,Z)
    private void SetPopupPosition()
    {
        if (textDisplay == null) return;
        textDisplay.transform.position = transform.position + offset;
    }
}
