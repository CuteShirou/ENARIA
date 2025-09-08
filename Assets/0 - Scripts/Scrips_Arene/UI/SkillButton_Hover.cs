using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButton_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Panel_MiniInfo_Bubble infoBubble;   // Référence vers la pop-up d'info
    public Data_Skill data;                    // Data du sort associé à ce bouton

    public void Init(Panel_MiniInfo_Bubble bubble, Data_Skill skill)
    {
        // Initialise les références depuis la SkillBar à l'instanciation
        infoBubble = bubble;
        data = skill;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Affiche la pop-up quand la souris entre sur le bouton
        if (infoBubble && data) infoBubble.Show(data, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // Met à jour la position pendant le survol
        if (infoBubble && data) infoBubble.Show(data, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Masque la pop-up quand la souris quitte le bouton
        if (infoBubble) infoBubble.Hide();
    }
}
