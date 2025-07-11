using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTooltipUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI specificationText;
    public RectTransform backgroundRect;
    public CanvasGroup canvasGroup;

    private Canvas parentCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        Hide();
    }

    public void Show(string title, string description, string specifications, Vector2 position)
    {
        titleText.text = title;
        descriptionText.text = description;
        bool hasSpecs = !string.IsNullOrEmpty(specifications);
        specificationText.gameObject.SetActive(hasSpecs);
        specificationText.text = hasSpecs ? specifications : "";

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, position, parentCanvas.worldCamera, out anchoredPos);

        float halfWidth = backgroundRect.rect.width * 0.5f;
        anchoredPos.x = Mathf.Clamp(anchoredPos.x,
            canvasRect.rect.xMin + halfWidth,
            canvasRect.rect.xMax - halfWidth);

        float halfHeight = backgroundRect.rect.height * 0.5f;
        anchoredPos.y = Mathf.Clamp(anchoredPos.y,
            canvasRect.rect.yMin + halfHeight,
            canvasRect.rect.yMax - halfHeight);

        backgroundRect.anchoredPosition = anchoredPos;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
