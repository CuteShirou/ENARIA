using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
public class FriendSearchUI : MonoBehaviour
{
    [Header("Références UI")]
    public TMP_InputField searchInput;
    public Transform resultsContainer;
    public GameObject searchListItemPrefab;

    public FriendsApi friendsApi;
    public int localPlayerId;

    void Start()
    {
        searchInput.onSubmit.AddListener(OnSearchSubmit);
    }

    void OnSearchSubmit(string searchText)
    {
        string trimmed = searchText.Trim();
        ClearResults();

        if (string.IsNullOrEmpty(trimmed))
        {
            Debug.Log("Recherche vide, rien à faire.");
            return;
        }

        Debug.Log($"Recherche d'utilisateur : {trimmed}");

        friendsApi.SearchUsers(
            localPlayerId,
            trimmed,
            users =>
            {
                if (users == null || users.Length == 0)
                {
                    Debug.Log("Aucun utilisateur trouvé.");
                    return;
                }

                var filteredUsers = users.Where(u => u.pseudo.ToLower().Contains(trimmed.ToLower())).ToArray();

                if (filteredUsers.Length == 0)
                {
                    Debug.Log("Aucun utilisateur trouvé après filtrage.");
                    return;
                }

                foreach (var u in filteredUsers)
                    CreateSearchResultItem(u.id, u.pseudo);
            },
            error =>
            {
                Debug.LogError("Erreur recherche utilisateurs: " + error);
            }
        );
    }

    void CreateSearchResultItem(int userId, string username)
    {
        GameObject itemGO = Instantiate(searchListItemPrefab, resultsContainer);

        TMP_Text usernameText = itemGO.transform.Find("UsernameTMP")?.GetComponent<TMP_Text>();
        Button addButton = itemGO.transform.Find("AddButton")?.GetComponent<Button>();

        if (usernameText != null)
            usernameText.text = username;

        if (addButton != null)
        {
            addButton.onClick.AddListener(() =>
            {
                friendsApi.SendFriendRequest(
                    localPlayerId,
                    userId,
                    () => Debug.Log($"Demande d'ami envoyée à {username} !"),
                    err => Debug.LogError("Erreur envoi demande ami : " + err)
                );
            });
        }
    }

    void ClearResults()
    {
        foreach (Transform child in resultsContainer)
            Destroy(child.gameObject);
    }
}
