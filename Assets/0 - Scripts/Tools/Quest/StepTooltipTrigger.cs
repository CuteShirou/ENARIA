using UnityEngine;
using UnityEngine.EventSystems;

public class StepTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public QuestStep step;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (step != null)
        {
            // On force currentProgress à 0 et on loggue la string générée
            string full = step.GetFullDescription(0);
            Debug.Log($"[StepTooltipTrigger] FullDesc = \n{full}");
            TooltipUI.Instance.Show(full);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }
}
