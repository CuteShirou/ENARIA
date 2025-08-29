using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ToggleImage : MonoBehaviour, IPointerClickHandler
{
    [Header("Image à changer lorsque sélectionné")]
    public Image targetImage;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Optionnel : taille personnalisée")]
    public bool useCustomSize = false;
    public Vector2 customSize = new Vector2(100, 100);

    private static List<ToggleImage> allElements = new List<ToggleImage>();

    private Vector2 originalSize;

    void Awake()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("TargetImage non assignée ! Le script ne fera rien.");
            return;
        }

        targetImage.sprite = normalSprite;
        originalSize = targetImage.rectTransform.sizeDelta;

        allElements.Add(this);
    }

    void OnDestroy()
    {
        allElements.Remove(this);
    }

    public void OnButtonClick()
    {
        SelectThis();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectThis();
    }

    private void SelectThis()
    {
        if (targetImage == null) return;

        foreach (var element in allElements)
        {
            element.ResetElement();
        }

        targetImage.sprite = selectedSprite;

        if (useCustomSize)
        {
            targetImage.rectTransform.sizeDelta = customSize;
        }
    }

    private void ResetElement()
    {
        if (targetImage == null) return;
        targetImage.sprite = normalSprite;

        targetImage.rectTransform.sizeDelta = originalSize;
    }
}
