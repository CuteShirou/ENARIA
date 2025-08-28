using UnityEngine;
using UnityEngine.UI;

public class ChatVisibilityToggle : MonoBehaviour
{
    [SerializeField] private GameObject target;

    [Header("Références")]
    [SerializeField] private RectTransform chatPanelRoot;
    [SerializeField] private RectTransform tabButton; 

    [Header("Positions")]
    [SerializeField] private Vector2 buttonVisiblePos = new Vector2(0, 0);
    [SerializeField] private Vector2 buttonHiddenPos = new Vector2(-200, 0);

    private bool isVisible = true;

    public void Toggle()
    {
        if (target != null)
            target.SetActive(!target.activeSelf);
    }

    public void ToggleChat()
    {
        isVisible = !isVisible;

        chatPanelRoot.gameObject.SetActive(isVisible);
        tabButton.anchoredPosition = isVisible ? buttonVisiblePos : buttonHiddenPos;
    }
}
