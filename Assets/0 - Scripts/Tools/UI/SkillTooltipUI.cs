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
            canvasRect, position,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out anchoredPos);

        float halfWidth = backgroundRect.rect.width * 0.5f;
        float halfHeight = backgroundRect.rect.height * 0.5f;

        anchoredPos.x = Mathf.Clamp(anchoredPos.x,
            -canvasRect.rect.width / 2f + halfWidth,
             canvasRect.rect.width / 2f - halfWidth);

        anchoredPos.y = Mathf.Clamp(anchoredPos.y,
            -canvasRect.rect.height / 2f + halfHeight,
             canvasRect.rect.height / 2f - halfHeight);

        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = anchoredPos;

    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
