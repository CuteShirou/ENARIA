using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Popup_DisplayNumber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas uiCanvas;                     // Canvas (Screen Space - Overlay recommandé)
    [SerializeField] private RectTransform panelDisplayNumber;    // Prefab Panel_DisplayNumber (RectTransform avec un enfant Text_InfoNumber TMP)

    [Header("Placement")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f); // Décalage monde (X, Y, Z) au-dessus de l’entité

    [Header("Timing")]
    [SerializeField] private float displayDuration = 1.5f;        // Durée d’affichage avant destruction

    [Header("Colors")]
    [SerializeField] private Color colorPA = new Color(0.25f, 0.6f, 1f); // Bleu PA
    [SerializeField] private Color colorPM = new Color(0.2f, 0.85f, 0.4f); // Vert PM
    [SerializeField] private Color colorPV = new Color(1f, 0.25f, 0.25f);  // Rouge PV (dégâts)

    // Registre global des instances UI créées, pour pouvoir tout nettoyer à la fin d’un combat
    private static readonly List<GameObject> activePopupInstances = new List<GameObject>();

    // ---------------------------------------------------------
    // DestroyAllActivePopups : détruit immédiatement toutes les pop-ups encore actives
    public static void DestroyAllActivePopups()
    {
        for (int i = 0; i < activePopupInstances.Count; i++)
        {
            var go = activePopupInstances[i];
            if (go != null) Object.Destroy(go);
        }
        activePopupInstances.Clear();
    }

    // ---------------------------------------------------------
    // SetupPopupReferences : permet d'injecter les références depuis Phase_EnterSetupCombat
    public void SetupPopupReferences(Canvas canvas, RectTransform prefab, Vector3 offset, float duration)
    {
        // Met à jour le Canvas si fourni
        if (canvas != null) uiCanvas = canvas;

        // Met à jour le prefab si fourni
        if (prefab != null) panelDisplayNumber = prefab;

        // Met à jour l'offset et la durée
        worldOffset = offset;
        displayDuration = duration;
    }

    // ---------------------------------------------------------
    // ShowPA : affiche "- X PA" en bleu
    public void ShowPA(int used)
    {
        string t = $"- {FormatNumber(used)} PA";
        ShowCustom(t, colorPA);
    }

    // ---------------------------------------------------------
    // ShowPM : affiche "- X PM" en vert
    public void ShowPM(int used)
    {
        string t = $"- {FormatNumber(used)} PM";
        ShowCustom(t, colorPM);
    }

    // ---------------------------------------------------------
    // ShowDamage : affiche "- X PV" en rouge (dégâts reçus)
    public void ShowDamage(int damage)
    {
        string t = $"- {FormatNumber(damage)} PV";
        ShowCustom(t, colorPV);
    }

    // ---------------------------------------------------------
    // ShowCustom : affiche un texte custom avec une couleur donnée
    public void ShowCustom(string text, Color color)
    {
        if (uiCanvas == null || panelDisplayNumber == null) return;

        // Instancie sous le Canvas (UI)
        RectTransform inst = Instantiate(panelDisplayNumber, uiCanvas.transform);
        inst.gameObject.SetActive(true);

        // Enregistre dans le registre global pour suppression groupée
        activePopupInstances.Add(inst.gameObject);

        // Récupère le TMP enfant nommé Text_InfoNumber
        TMP_Text txt = inst.GetComponentInChildren<TMP_Text>(true);
        if (txt != null)
        {
            txt.text = text;
            txt.color = color;
        }

        // Lance le suivi position + destruction auto
        StartCoroutine(Co_DisplayAndFollow(inst));
    }

    // ---------------------------------------------------------
    // Co_DisplayAndFollow : place/maintient la popup au-dessus de l’entité puis détruit
    private IEnumerator Co_DisplayAndFollow(RectTransform uiInstance)
    {
        float t = 0f;

        // Boucle pendant la durée d’affichage
        while (t < displayDuration && uiInstance != null)
        {
            UpdateUIPosition(uiInstance);
            t += Time.deltaTime;
            yield return null;
        }

        // Retire du registre (si pas déjà détruite par un nettoyage global)
        if (uiInstance != null)
        {
            activePopupInstances.Remove(uiInstance.gameObject);
            Destroy(uiInstance.gameObject);
        }
    }

    // ---------------------------------------------------------
    // UpdateUIPosition : met à jour la position écran de la popup
    private void UpdateUIPosition(RectTransform uiInstance)
    {
        if (uiCanvas == null || uiInstance == null || Camera.main == null) return;

        Vector3 worldPos = transform.position + worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Convertit écran → local du Canvas (Overlay = cam null)
        RectTransform canvasRect = uiCanvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 local);
        uiInstance.anchoredPosition = local;
    }

    // ---------------------------------------------------------
    // FormatNumber : formate les nombres avec des espaces (ex: 1 500)
    private string FormatNumber(int value)
    {
        string s = value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
        return s.Replace('\u00A0', ' ');
    }
}
