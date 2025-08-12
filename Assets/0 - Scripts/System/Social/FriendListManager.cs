using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FriendListManager : MonoBehaviour
{
    [Header("References")]
    public Transform friendListContainer;
    public GameObject friendItemPrefab;
    public AvatarSelectorUI avatarSelectorUI;

    [Header("API & Player")]
    public FriendsApi friendsApi;
    public int playerId;

    private List<FriendData> currentFriends = new List<FriendData>();

    void Start()
    {
        LoadFriendsFromApi();
    }

    private Sprite GetAvatarSpriteByName(string avatarName)
    {
        if (string.IsNullOrEmpty(avatarName)) return null;

        foreach (var sprite in avatarSelectorUI.availableAvatars)
        {
            if (sprite != null && sprite.name == avatarName)
                return sprite;
        }

        Debug.LogWarning($"Avatar non trouvé dans availableAvatars : {avatarName}");
        return null;
    }

    public void LoadFriendsFromApi()
    {
        friendsApi.GetFriends(playerId, (friends) =>
        {
            var friendDataList = new List<FriendData>();

            foreach (var f in friends)
            {
                Sprite avatarSprite = GetAvatarSpriteByName(f.avatar);

                friendDataList.Add(new FriendData
                {
                    id = f.id,
                    username = f.pseudo,
                    isFavorite = PlayerPrefs.GetInt("fav_" + f.pseudo, f.is_favorite ? 1 : 0) == 1,
                    isOnline = false,
                    avatar = avatarSprite
                });
            }

            LoadFriendList(friendDataList);
            UpdateOnlineStatus();
        }, (error) =>
        {
            Debug.LogError("Erreur chargement amis : " + error);
        });
    }

    public void LoadFriendList(List<FriendData> friends)
    {
        currentFriends = friends;
        RefreshUI();
    }

    public void AddFriend(FriendData newFriend)
    {
        if (currentFriends.Any(f => f.id == newFriend.id))
            return;

        currentFriends.Add(newFriend);
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in friendListContainer)
            Destroy(child.gameObject);

        var sorted = currentFriends
            .OrderByDescending(f => f.isFavorite)
            .ThenBy(f => f.username)
            .ToList();

        foreach (var friend in sorted)
        {
            var itemGO = Instantiate(friendItemPrefab, friendListContainer);
            var itemUI = itemGO.GetComponent<FriendListItemUI>();

            itemUI.Setup(friend.username, friend.isFavorite, friend.isOnline, friend.avatar);

            itemUI.onFavoriteChanged = (username, newValue) =>
            {
                var target = currentFriends.FirstOrDefault(x => x.username == username);
                if (target == null) return;

                target.isFavorite = newValue;
                PlayerPrefs.SetInt("fav_" + username, newValue ? 1 : 0);
                PlayerPrefs.Save();

                friendsApi.ToggleFavorite(playerId, target.id, newValue, (success) =>
                {
                    Debug.Log($"[Favori] {username} => {newValue}");
                    RefreshUI();
                }, (error) =>
                {
                    Debug.LogError("Erreur toggle favori API : " + error);
                });
            };

            itemUI.onRemoveFriend = (username) =>
            {
                var friendToRemove = currentFriends.FirstOrDefault(f => f.username == username);
                if (friendToRemove == null) return;

                Debug.Log($"[SUPPRIMER] Suppression de l’ami {username}");

                friendsApi.RemoveFriend(playerId, friendToRemove.id, () =>
                {
                    currentFriends.Remove(friendToRemove);
                    RefreshUI();
                }, (error) =>
                {
                    Debug.LogError("Erreur suppression ami API : " + error);
                });
            };
        }
    }

    public void UpdateOnlineStatus()
    {
        friendsApi.GetOnlineFriends(playerId, (onlineIds) =>
        {
            foreach (var friend in currentFriends)
            {
                friend.isOnline = onlineIds.Contains(friend.id);
            }
            RefreshUI();
        }, (error) =>
        {
            Debug.LogWarning("Erreur récupération statut en ligne : " + error);
        });
    }
}
