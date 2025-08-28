using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaHitButton : MonoBehaviour
{
    [Range(0f, 1f)]
    public float threshold = 0.1f; // 10% d’alpha pour considérer “plein”

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        img.alphaHitTestMinimumThreshold = threshold;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!img) img = GetComponent<Image>();
        if (img) img.alphaHitTestMinimumThreshold = threshold;
    }
#endif
}
