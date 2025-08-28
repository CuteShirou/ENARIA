using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;

        if (tooltipPanel != null)
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        else
            Debug.LogError("TooltipPanel n’est pas assigné !");
    }

    private void Update()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipPanel.transform.parent.GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out pos);
tooltipPanel.GetComponent<RectTransform>().anchoredPosition = pos + new Vector2(50, -50);
        }
    }

    public void Show(string content)
    {
        Debug.Log("Tooltip Show: " + content);
        tooltipText.text = content;

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());

        RectTransform canvasRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null,
            out pos);

        tooltipRect.anchoredPosition = pos + new Vector2(10, -10);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }


    public void Hide()
    {
        Debug.Log("Tooltip Hide");
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}
