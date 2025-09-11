using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Popup_DisplayNumber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas uiCanvas;                     // Canvas d'affichage UI
    [SerializeField] private RectTransform panelDisplayNumber;    // Prefab Panel_DisplayNumber (RectTransform avec un enfant Text_InfoNumber TMP)

    [Header("Placement")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f); // Décalage monde au-dessus de l’entité

    [Header("Timing")]
    [SerializeField] private float displayDuration = 1.5f;        // Durée d’affichage avant destruction

    [Header("Colors")]
    [SerializeField] private Color colorPA = new Color(0.25f, 0.6f, 1f);  // Bleu PA
    [SerializeField] private Color colorPM = new Color(0.2f, 0.85f, 0.4f); // Vert PM
    [SerializeField] private Color colorPV = new Color(1f, 0.25f, 0.25f);  // Rouge PV (dégâts)

    // Registre global des instances UI créées (nettoyage fin de combat)
    private static readonly List<GameObject> activePopupInstances = new List<GameObject>();

    // Registre PAR ENTITÉ des popups issues de CE composant (pour attendre/forcer la fermeture)
    private readonly List<GameObject> myPopupInstances = new List<GameObject>();

    // Runner global pour faire tourner les coroutines indépendamment de l'état de l'entité
    private static PopupRunner runner;
    private static PopupRunner Runner
    {
        get
        {
            if (runner == null)
            {
                var go = new GameObject("__PopupRunner");
                Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<PopupRunner>();
            }
            return runner;
        }
    }

    // Composant vide servant uniquement d'hôte aux coroutines
    private sealed class PopupRunner : MonoBehaviour { }

    // ---------------------------------------------------------
    // DestroyAllActivePopups : détruit immédiatement toutes les pop-ups encore actives (global)
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
    // SetupPopupReferences : injection depuis l'extérieur si besoin
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
        RectTransform inst = Object.Instantiate(panelDisplayNumber, uiCanvas.transform);
        inst.gameObject.SetActive(true);

        // Registres global + local
        activePopupInstances.Add(inst.gameObject);
        myPopupInstances.Add(inst.gameObject);

        // Récupère le TMP enfant nommé Text_InfoNumber
        TMP_Text txt = inst.GetComponentInChildren<TMP_Text>(true);
        if (txt != null)
        {
            txt.text = text;
            txt.color = color;
        }

        // Lance le suivi position + destruction auto via le runner global
        Runner.StartCoroutine(Co_DisplayAndFollow(inst));
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

        // Retire des registres et détruit l'instance si encore présente
        if (uiInstance != null)
        {
            activePopupInstances.Remove(uiInstance.gameObject);
            myPopupInstances.Remove(uiInstance.gameObject);
            Object.Destroy(uiInstance.gameObject);
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
    // HasActivePopups : indique si CE lanceur a encore des popups visibles
    public bool HasActivePopups()
    {
        // Nettoie les entrées nulles au passage
        for (int i = myPopupInstances.Count - 1; i >= 0; i--)
        {
            if (myPopupInstances[i] == null) myPopupInstances.RemoveAt(i);
        }
        return myPopupInstances.Count > 0;
    }

    // ---------------------------------------------------------
    // WaitMyPopupsToFinish : attend que TOUTES les popups de CE lanceur soient détruites
    public IEnumerator WaitMyPopupsToFinish()
    {
        // Boucle jusqu'à ce que toutes mes popups soient parties
        while (HasActivePopups())
            yield return null;
    }

    // ---------------------------------------------------------
    // ForceCloseMyPopups : détruit immédiatement TOUTES les popups de CE lanceur
    public void ForceCloseMyPopups()
    {
        for (int i = 0; i < myPopupInstances.Count; i++)
        {
            var go = myPopupInstances[i];
            if (go != null) Object.Destroy(go);
        }
        myPopupInstances.Clear();
    }

    // ---------------------------------------------------------
    // WaitAllPopupsToFinish : (optionnel) attend la fin de toutes les popups globales
    public static IEnumerator WaitAllPopupsToFinish()
    {
        // Nettoie global de temps en temps
        while (true)
        {
            for (int i = activePopupInstances.Count - 1; i >= 0; i--)
            {
                if (activePopupInstances[i] == null) activePopupInstances.RemoveAt(i);
            }
            if (activePopupInstances.Count == 0) yield break;
            yield return null;
        }
    }

    // ---------------------------------------------------------
    // RunDisableAfterPopups : utilitaire prêt-à-l'emploi
    // Désactive 'toDisable' après la fin de toutes les popups du 'owner' (utilise le runner global)
    public static void RunDisableAfterPopups(Popup_DisplayNumber owner, GameObject toDisable)
    {
        if (toDisable == null) return;

        Runner.StartCoroutine(Co_RunDisable(owner, toDisable));

        IEnumerator Co_RunDisable(Popup_DisplayNumber o, GameObject go)
        {
            if (o != null) yield return o.WaitMyPopupsToFinish();
            if (go != null) go.SetActive(false);
        }
    }

    // ---------------------------------------------------------
    // FormatNumber : formate les nombres avec des espaces (ex: 1 500)
    private string FormatNumber(int value)
    {
        string s = value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
        return s.Replace('\u00A0', ' ');
    }
}
