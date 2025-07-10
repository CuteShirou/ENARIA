using UnityEngine;

public class TimelineSlotUIController : MonoBehaviour
{
    [Header("Offset à décaler pour le visuel")]
    public RectTransform offsetTransform;  // Référence à l’objet enfant "Offset"
    private Vector2 originalPosition;

    [Header("Paramètres")]
    public float shiftAmount = 30f; // Combien de pixels vers la droite

    private void Start()
    {
        if (offsetTransform != null)
        {
            originalPosition = offsetTransform.anchoredPosition;
        }
        else
        {
            Debug.LogWarning("OffsetTransform n’est pas assigné dans TimelineSlotUIController.");
        }
    }

    public void SetActiveTurn(bool isActive)
    {
        if (offsetTransform == null) return;

        if (isActive)
        {
            offsetTransform.anchoredPosition = originalPosition + new Vector2(shiftAmount, 0f);
        }
        else
        {
            offsetTransform.anchoredPosition = originalPosition;
        }
    }
}
