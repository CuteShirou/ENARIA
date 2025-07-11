using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoBubbleUI : MonoBehaviour
{
    [Header("Portrait & Texte")]
    public Image portrait;
    public TMP_Text pseudoText;
    public TMP_Text levelText;

    [Header("Stats")]
    public TMP_Text paText;
    public TMP_Text pmText;
    public TMP_Text hpCurrentText;
    public TMP_Text hpMaxText;

    [Header("Résistances")]
    public TMP_Text resForText;
    public TMP_Text resDexText;
    public TMP_Text resMagText;
    public TMP_Text resFoiText;

    [Header("Barre de vie")]
    public Image hpBarFill;

    public void SetInfo(CombatStats stats, Sprite portraitSprite, string pseudo, int level)
    {
        pseudoText.text = pseudo;
        levelText.text = level.ToString();

        paText.text = stats.currentPA.ToString();
        pmText.text = stats.currentPM.ToString();

        hpCurrentText.text = stats.currentHP.ToString();
        hpMaxText.text = stats.baseHP.ToString();

        resForText.text = stats.GetResistance(SkillElement.Force).ToString();
        resDexText.text = stats.GetResistance(SkillElement.Dexterité).ToString();
        resMagText.text = stats.GetResistance(SkillElement.Magie).ToString();
        resFoiText.text = stats.GetResistance(SkillElement.Foi).ToString();

        hpBarFill.fillAmount = (float)stats.currentHP / stats.baseHP;

        if (portrait != null)
            portrait.sprite = portraitSprite;
    }
}
