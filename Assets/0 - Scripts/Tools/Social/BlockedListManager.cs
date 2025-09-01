using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BlockedListManager : MonoBehaviour
{
    [Header("References")]
    public Transform blockedListContainer;
    public GameObject blockedItemPrefab;

    [Header("API & Player")]
    public FriendsApi friendsApi;
    public int playerId;

    private List<FriendsApi.Friend> currentBlocked = new();

    void Start()
    {
        LoadBlockedFromApi();
    }

    void LoadBlockedFromApi()
    {
        friendsApi.GetBlockedFriends(playerId, blocked =>
        {
            currentBlocked = blocked.ToList();
            RefreshUI();
        }, error =>
        {
            Debug.LogError("Erreur chargement bloqués : " + error);
        });
    }

    void RefreshUI()
    {
        foreach (Transform child in blockedListContainer)
            Destroy(child.gameObject);

        foreach (var f in currentBlocked)
        {
            var itemGO = Instantiate(blockedItemPrefab, blockedListContainer);
            var itemUI = itemGO.GetComponent<BlockedUserItemUI>();
            itemUI.Setup(f.pseudo, () => OnUnblock(f, itemGO));
        }
    }

    void OnUnblock(FriendsApi.Friend friend, GameObject itemGO)
    {
        friendsApi.RemoveFriend(playerId, friend.id, () =>
        {
            Debug.Log($"Débloqué : {friend.pseudo}");
            currentBlocked.Remove(friend);
            Destroy(itemGO);
        }, error =>
        {
            Debug.LogError("Erreur débloquage : " + error);
        });
    }
}
