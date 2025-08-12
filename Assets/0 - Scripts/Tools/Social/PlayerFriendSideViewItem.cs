using UnityEngine;
using UnityEngine.UI;

public class PlayerFriendSideViewItem : MonoBehaviour
{
    public Image avatarImage;
    public Button avatarButton;
    public AvatarSelectorUI avatarSelectorUI;

    [Header("API Settings")]
    public int playerId;
    public FriendsApi friendsApi;

    void Start()
    {
        if (avatarButton != null)
            avatarButton.onClick.AddListener(OnAvatarClicked);
    }

    public void OnAvatarClicked()
    {
        if (avatarSelectorUI == null)
        {
            Debug.LogWarning("AvatarSelectorUI non assigné !");
            return;
        }

        avatarSelectorUI.Open(selectedSprite =>
        {
            avatarImage.sprite = selectedSprite;
            string avatarName = selectedSprite.name;
            Debug.Log("Avatar sélectionné : " + avatarName);

            if (friendsApi != null)
            {
                friendsApi.UpdateAvatar(
                    playerId,
                    avatarName,
                    onSuccess: () => Debug.Log("Avatar mis à jour en BDD"),
                    onError: err => Debug.LogError("Erreur UpdateAvatar : " + err)
                );
            }
            else
            {
                Debug.LogWarning("FriendsApi non assigné !");
            }
        });
    }
}
