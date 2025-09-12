using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI_Principal_HUD
///   Met à jour en temps réel l'UI principale : PA / PM / PO et la barre de PV (Image_CurrentHP).
///      Lit directement les valeurs sur Entity_StatistiqueCombat du joueur.
/// </summary>
public class UI_InfoPrincipal : MonoBehaviour
{
    [Header("Target (drag & drop votre Player)")]
    [SerializeField] private Entity_StatistiqueCombat statsTarget;
    //   Cible contenant les stats courantes (currentHP, baseHP, currentPA/PM/PO)

    [Header("HP Bar (Image)")]
    [SerializeField] private Image imageMaxHP;      //   Optionnel (sert de largeur de référence si on n'utilise pas fillAmount)
    [SerializeField] private Image imageCurrentHP;  //   Doit être Image Type = Filled, Method = Horizontal, Origin = Left
    [SerializeField] private bool useImageFillAmount = true;
    //   true = on pilote imageCurrentHP.fillAmount (recommandé)
    //      false = on ajuste la largeur (RectTransform) de l'image

    [Header("Texts PA / PM / PO")]
    [SerializeField] private TMP_Text valuePA;
    [SerializeField] private TMP_Text valuePM;
    [SerializeField] private TMP_Text valuePO;

    // --------- Caches internes pour éviter les refreshs inutiles ---------
    private int lastHP = int.MinValue;
    private int lastHPMax = int.MinValue;
    private int lastPA = int.MinValue;
    private int lastPM = int.MinValue;
    private int lastPO = int.MinValue;
    private float fullHpBarWidth = -1f;

    private void Awake()
    {
        //   Si on n'utilise pas le fillAmount, on mémorise la largeur "pleine" de référence
        if (!useImageFillAmount && imageCurrentHP != null)
        {
            RectTransform refRect = imageMaxHP ? imageMaxHP.rectTransform : imageCurrentHP.rectTransform;
            fullHpBarWidth = refRect.rect.width;
        }

        //   Force un premier rafraîchissement
        ForceRefresh();
    }

    private void OnEnable()
    {
        //   Re-synchronise à l'activation
        ForceRefresh();
    }

    private void Update()
    {
        //   Aucune mise à jour si la cible n'est pas assignée
        if (!statsTarget) return;

        //   Lecture des valeurs courantes (on suppose les noms suivants existent déjà dans votre script de stats)
        int hp = Mathf.RoundToInt(statsTarget.currentHP);
        int hpMax = Mathf.Max(1, Mathf.RoundToInt(statsTarget.baseHP));
        int pa = Mathf.Max(0, statsTarget.currentPA);
        int pm = Mathf.Max(0, statsTarget.currentPM);
        int po = Mathf.Max(0, statsTarget.currentPO);

        //   HP (barre)
        if (hp != lastHP || hpMax != lastHPMax)
        {
            UpdateHpBar(hp, hpMax);
            lastHP = hp;
            lastHPMax = hpMax;
        }

        //   PA
        if (pa != lastPA)
        {
            if (valuePA) valuePA.text = pa.ToString();
            lastPA = pa;
        }

        //   PM
        if (pm != lastPM)
        {
            if (valuePM) valuePM.text = pm.ToString();
            lastPM = pm;
        }

        //   PO
        if (po != lastPO)
        {
            if (valuePO) valuePO.text = po.ToString();
            lastPO = po;
        }
    }

    /// <summary>
    /// ForceRefresh
    ///   Force la mise à jour immédiate
    /// </summary>
    public void ForceRefresh()
    {
        lastHP = int.MinValue;
        lastHPMax = int.MinValue;
        lastPA = int.MinValue;
        lastPM = int.MinValue;
        lastPO = int.MinValue;
        Update();
    }

    /// <summary>
    /// UpdateHpBar
    ///   Met à jour visuellement la barre de PV.
    ///      Pour que la barre DIMINUE de la DROITE vers la GAUCHE :
    ///      - Mettre imageCurrentHP en Image Type = Filled
    ///      - Fill Method = Horizontal
    ///      - Fill Origin = Left
    /// </summary>
    private void UpdateHpBar(int hp, int hpMax)
    {
        if (!imageCurrentHP) return;

        float ratio = Mathf.Clamp01(hp / (float)hpMax);

        //   Méthode 1 : pilotage du fillAmount
        if (useImageFillAmount && imageCurrentHP.type == Image.Type.Filled)
        {
            imageCurrentHP.fillAmount = ratio;
            return;
        }

        //   Méthode 2 : ajustement de la largeur si on ne veut pas utiliser le fill
        if (fullHpBarWidth < 0f)
        {
            RectTransform refRect = imageMaxHP ? imageMaxHP.rectTransform : imageCurrentHP.rectTransform;
            fullHpBarWidth = refRect.rect.width;
        }

        var rt = imageCurrentHP.rectTransform;
        var size = rt.sizeDelta;
        size.x = fullHpBarWidth * ratio;
        rt.sizeDelta = size;
    }
}
