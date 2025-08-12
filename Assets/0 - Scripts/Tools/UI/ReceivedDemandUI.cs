using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReceivedDemandUI : MonoBehaviour
{
    [Header("Références UI")]
    public Transform demandsContainer;
    public GameObject demandItemPrefab;

    [Header("Gestionnaire d'amis & API")]
    public FriendListManager friendListManager;
    public FriendsApi friendsApi;

    public int playerId;

    void Start()
    {
        LoadDemandsFromApi();
    }

    public void LoadDemandsFromApi()
    {
        ClearDemands();

        friendsApi.GetPendingRequests(playerId, OnRequestsReceived, OnRequestsError);
    }

    private void OnRequestsReceived(FriendsApi.FriendRequest[] requests)
    {
        foreach (var request in requests)
        {
            CreateDemandItem(request.pseudo, request.id);
        }
    }

    private void OnRequestsError(string error)
    {
        Debug.LogError("Erreur en chargeant les demandes : " + error);
    }

    void CreateDemandItem(string username, int requesterId)
    {
        GameObject itemGO = Instantiate(demandItemPrefab, demandsContainer);

        TMP_Text usernameText = itemGO.transform.Find("UsernameTMP")?.GetComponent<TMP_Text>();
        Button acceptButton = itemGO.transform.Find("AcceptButton")?.GetComponent<Button>();
        Button refuseButton = itemGO.transform.Find("RefuseButton")?.GetComponent<Button>();

        if (usernameText != null)
            usernameText.text = username;

        if (acceptButton != null)
            acceptButton.onClick.AddListener(() => OnAccept(requesterId, itemGO, username));
        else
            Debug.LogWarning("AcceptButton pas trouvé dans le prefab");

        if (refuseButton != null)
            refuseButton.onClick.AddListener(() => OnRefuse(requesterId, itemGO, username));
        else
            Debug.LogWarning("RefuseButton pas trouvé dans le prefab");
    }

    void OnAccept(int requesterId, GameObject itemGO, string username)
    {
        friendsApi.RespondFriendRequest(playerId, requesterId, "accept", () =>
        {
            Debug.Log($"Demande d'ami acceptée de {username}");

            if (friendListManager != null)
            {
                friendListManager.AddFriend(new FriendData
                {
                    username = username,
                    isOnline = false,
                    avatar = null,
                    isFavorite = false
                });
            }

            Destroy(itemGO);

        }, (error) =>
        {
            Debug.LogError("Erreur en acceptant la demande : " + error);
        });
    }

    void OnRefuse(int requesterId, GameObject itemGO, string username)
    {
        friendsApi.RespondFriendRequest(playerId, requesterId, "reject", () =>
        {
            Debug.Log($"Demande d'ami refusée de {username}");
            Destroy(itemGO);
        }, (error) =>
        {
            Debug.LogError("Erreur en refusant la demande : " + error);
        });
    }

    void ClearDemands()
    {
        foreach (Transform child in demandsContainer)
            Destroy(child.gameObject);
    }
}
