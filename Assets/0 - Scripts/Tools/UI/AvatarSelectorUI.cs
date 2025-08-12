using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AvatarSelectorUI : MonoBehaviour
{
    public GameObject avatarItemPrefab;
    public Transform gridContainer;
    public Button closeButton;

    public List<Sprite> availableAvatars;

    private System.Action<Sprite> onAvatarSelected;

    void Start()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Open(System.Action<Sprite> callback)
    {
        onAvatarSelected = callback;
        gameObject.SetActive(true);

        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        foreach (var avatar in availableAvatars)
        {
            GameObject avatarGO = Instantiate(avatarItemPrefab, gridContainer);
            avatarGO.GetComponent<Image>().sprite = avatar;
            avatarGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                onAvatarSelected?.Invoke(avatar);
                gameObject.SetActive(false);
            });
        }
    }
}
