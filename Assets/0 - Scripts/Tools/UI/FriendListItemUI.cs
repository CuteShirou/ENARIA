using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendListItemUI : MonoBehaviour
{
    public System.Action<string, bool> onFavoriteChanged;

    public TMP_Text usernameTMP;
    public Image statusIcon;
    public Button inviteButton;
    public Button removeFriendButton;
    public Button favoriteButton;
    public Image favoriteIconImage;
    public Image avatarImage;
    public Sprite favoriIconFilled;
    public Sprite favoriIconEmpty;
    public Sprite defaultAvatarSprite;

    private bool isFavorite = false;
    private string friendUsername;

    public System.Action<string> onRemoveFriend;

    public void Setup(string username, bool favorite, bool isOnline, Sprite avatar = null)
    {
        friendUsername = username;
        usernameTMP.text = username;
        isFavorite = favorite;

        if (avatarImage != null)
        {
            if (avatar != null)
                avatarImage.sprite = avatar;
            else
                avatarImage.sprite = defaultAvatarSprite;
        }

        favoriteButton.onClick.RemoveAllListeners();

        removeFriendButton.onClick.RemoveAllListeners();
        removeFriendButton.onClick.AddListener(() =>
        {
            onRemoveFriend?.Invoke(friendUsername);
        });

        UpdateFavoriIcon();
        UpdateStatus(isOnline);
    }


    public void ToggleFavorite()
    {
        isFavorite = !isFavorite;
        UpdateFavoriIcon();
        onFavoriteChanged?.Invoke(friendUsername, isFavorite);
    }

    private void UpdateFavoriIcon()
    {
        favoriteIconImage.sprite = isFavorite ? favoriIconFilled : favoriIconEmpty;
    }

    private void UpdateStatus(bool isOnline)
    {
        statusIcon.color = isOnline ? Color.green : Color.gray;
    }
}
