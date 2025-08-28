//using System;
//using System.Collections;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//public class FriendsApi : MonoBehaviour
//{
//    [Serializable]
//    public class Friend
//    {
//        public int id;
//        public string pseudo;
//        public string avatar;
//        public bool is_favorite;
//    }

//    [Serializable]
//    public class FriendsResponse
//    {
//        public bool success;
//        public Friend[] friends;
//        public string error;
//    }

//    [Serializable]
//    private class SimpleResponse
//    {
//        public bool success;
//        public string message;
//        public string error;
//    }

//    [Serializable]
//    public class FriendRequest
//    {
//        public int id;
//        public string pseudo;
//        public string requestedAt;
//    }

//    [Serializable]
//    private class FriendRequestsResponse
//    {
//        public bool success;
//        public FriendRequest[] requests;
//        public string error;
//    }

//    [Serializable]
//    public class UserInfo
//    {
//        public int id;
//        public string pseudo;
//    }

//    [Serializable]
//    private class SearchUsersResponse
//    {
//        public bool success;
//        public UserInfo[] users;
//        public string error;
//    }

//    [Serializable]
//    private class OnlineFriendsResponse
//    {
//        public bool success;
//        public int[] online_friends;
//        public string error;
//    }

//    [Serializable]
//    private class BlockedFriendsResponse
//    {
//        public bool success;
//        public Friend[] blocked;
//        public string error;
//    }

//    private string baseUrl = "https://enaria.nexus-com.fr/";
//    private string apiUser = "enaria@nexus-com.fr";
//    private string apiPass = "EnariaAdmin";


//    public void GetFriends(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
//    {
//        StartCoroutine(GetFriendsCoroutine(playerId, onSuccess, onError));
//    }

//    private IEnumerator GetFriendsCoroutine(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
//    {
//        string url = $"{baseUrl}get_friends_list.php?player_id={playerId}";
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<FriendsResponse>(www.downloadHandler.text);
//                if (response.success)
//                    onSuccess?.Invoke(response.friends);
//                else
//                    onError?.Invoke(response.error ?? "Unknown server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void SendFriendRequest(int playerId, int targetId, Action onSuccess, Action<string> onError)
//        => StartCoroutine(SendFriendRequestCoroutine(playerId, targetId, onSuccess, onError));

//    private IEnumerator SendFriendRequestCoroutine(int playerId, int targetId, Action onSuccess, Action<string> onError)
//    {
//        string url = baseUrl + "send_friend_request.php";
//        WWWForm form = new WWWForm();
//        form.AddField("player_id", playerId);
//        form.AddField("target_id", targetId);

//        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<SimpleResponse>(www.downloadHandler.text);
//                if (response.success) onSuccess?.Invoke();
//                else onError?.Invoke(response.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void RemoveFriend(int playerId, int targetId, Action onSuccess, Action<string> onError)
//        => StartCoroutine(RemoveFriendCoroutine(playerId, targetId, onSuccess, onError));

//    private IEnumerator RemoveFriendCoroutine(int playerId, int targetId, Action onSuccess, Action<string> onError)
//    {
//        string url = baseUrl + "remove_friend.php";
//        WWWForm form = new WWWForm();
//        form.AddField("player_id", playerId);
//        form.AddField("target_id", targetId);

//        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<SimpleResponse>(www.downloadHandler.text);
//                if (response.success) onSuccess?.Invoke();
//                else onError?.Invoke(response.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void GetPendingRequests(int playerId, Action<FriendRequest[]> onSuccess, Action<string> onError)
//        => StartCoroutine(GetPendingRequestsCoroutine(playerId, onSuccess, onError));

//    private IEnumerator GetPendingRequestsCoroutine(int playerId, Action<FriendRequest[]> onSuccess, Action<string> onError)
//    {
//        string url = $"{baseUrl}get_friend_requests.php?player_id={playerId}";
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<FriendRequestsResponse>(www.downloadHandler.text);
//                if (response.success) onSuccess?.Invoke(response.requests);
//                else onError?.Invoke(response.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void RespondFriendRequest(int playerId, int requesterId, string action, Action onSuccess, Action<string> onError)
//        => StartCoroutine(RespondFriendRequestCoroutine(playerId, requesterId, action, onSuccess, onError));

//    private IEnumerator RespondFriendRequestCoroutine(int playerId, int requesterId, string action, Action onSuccess, Action<string> onError)
//    {
//        string url = baseUrl + "respond_friend_request.php";
//        WWWForm form = new WWWForm();
//        form.AddField("player_id", playerId);
//        form.AddField("requester_id", requesterId);
//        form.AddField("action", action);

//        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var resp = JsonUtility.FromJson<SimpleResponse>(www.downloadHandler.text);
//                if (resp.success) onSuccess?.Invoke();
//                else onError?.Invoke(resp.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void ToggleFavorite(int playerId, int targetId, bool isFavorite, Action<bool> onSuccess, Action<string> onError)
//        => StartCoroutine(ToggleFavoriteCoroutine(playerId, targetId, isFavorite, onSuccess, onError));

//    private IEnumerator ToggleFavoriteCoroutine(int playerId, int targetId, bool isFavorite, Action<bool> onSuccess, Action<string> onError)
//    {
//        string url = baseUrl + "toggle_favorite.php";
//        WWWForm form = new WWWForm();
//        form.AddField("player_id", playerId);
//        form.AddField("target_id", targetId);
//        form.AddField("is_favorite", isFavorite ? 1 : 0);

//        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var resp = JsonUtility.FromJson<SimpleResponse>(www.downloadHandler.text);
//                if (resp.success) onSuccess?.Invoke(isFavorite);
//                else onError?.Invoke(resp.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void SearchUsers(int playerId, string searchText, Action<UserInfo[]> onSuccess, Action<string> onError)
//        => StartCoroutine(SearchUsersCoroutine(playerId, searchText, onSuccess, onError));

//    private IEnumerator SearchUsersCoroutine(int playerId, string searchText, Action<UserInfo[]> onSuccess, Action<string> onError)
//    {
//        string url = $"{baseUrl}search_users.php?player_id={playerId}&search={UnityWebRequest.EscapeURL(searchText)}";
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<SearchUsersResponse>(www.downloadHandler.text);
//                if (response.success) onSuccess?.Invoke(response.users);
//                else onError?.Invoke(response.error ?? "Server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }

//    public void GetOnlineFriends(int playerId, Action<int[]> onSuccess, Action<string> onError)
//        => StartCoroutine(GetOnlineFriendsCoroutine(playerId, onSuccess, onError));

//    private IEnumerator GetOnlineFriendsCoroutine(int playerId, Action<int[]> onSuccess, Action<string> onError)
//    {
//        string url = $"{baseUrl}get_online_friends.php?player_id={playerId}";
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<OnlineFriendsResponse>(www.downloadHandler.text);
//                if (response.success) onSuccess?.Invoke(response.online_friends);
//                else onError?.Invoke(response.error ?? "Erreur serveur");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("Erreur JSON : " + ex.Message);
//            }
//        }
//    }

//    public void GetBlockedFriends(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
//    {
//        StartCoroutine(GetBlockedFriendsCoroutine(playerId, onSuccess, onError));
//    }

//    private IEnumerator GetBlockedFriendsCoroutine(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
//    {
//        string url = $"{baseUrl}get_blocked_friends.php?player_id={playerId}";
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var response = JsonUtility.FromJson<BlockedFriendsResponse>(www.downloadHandler.text);
//                if (response.success)
//                    onSuccess?.Invoke(response.blocked);
//                else
//                    onError?.Invoke(response.error ?? "Unknown server error");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("JSON parsing error: " + ex.Message);
//            }
//        }
//    }
//    public void UpdateAvatar(int playerId, string avatarName, Action onSuccess, Action<string> onError)
//    {
//        StartCoroutine(UpdateAvatarCoroutine(playerId, avatarName, onSuccess, onError));
//    }

//    private IEnumerator UpdateAvatarCoroutine(int playerId, string avatarName, Action onSuccess, Action<string> onError)
//    {
//        string url = baseUrl + "update_avatar.php";
//        WWWForm form = new WWWForm();
//        form.AddField("player_id", playerId);
//        form.AddField("avatar", avatarName);

//        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//        {
//            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiPass}"));
//            www.SetRequestHeader("Authorization", "Basic " + credentials);

//            yield return www.SendWebRequest();

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                onError?.Invoke(www.error);
//                yield break;
//            }

//            try
//            {
//                var resp = JsonUtility.FromJson<SimpleResponse>(www.downloadHandler.text);
//                if (resp.success) onSuccess?.Invoke();
//                else onError?.Invoke(resp.error ?? "Erreur serveur");
//            }
//            catch (Exception ex)
//            {
//                onError?.Invoke("Erreur JSON: " + ex.Message);
//            }
//        }
//    }
//}







using System;
using System.Collections;
using UnityEngine;

public class FriendsApi : MonoBehaviour
{
    [Serializable]
    public class Friend
    {
        public int id;
        public string pseudo;
        public string avatar;
        public bool is_favorite;
    }

    [Serializable]
    public class FriendRequest
    {
        public int id;
        public string pseudo;
        public string requestedAt;
    }

    [Serializable]
    public class UserInfo
    {
        public int id;
        public string pseudo;
    }


    public void GetFriends(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            var fakeFriends = new Friend[]
            {
                new Friend { id = 1, pseudo = "Alice",   avatar = "avatar1", is_favorite = true },
                new Friend { id = 2, pseudo = "Bob",     avatar = "avatar2", is_favorite = false },
                new Friend { id = 3, pseudo = "Charlie", avatar = "avatar3", is_favorite = true }
            };
            onSuccess?.Invoke(fakeFriends);
        }));
    }

    public void SendFriendRequest(int playerId, int targetId, Action onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            onSuccess?.Invoke();
        }));
    }

    public void RemoveFriend(int playerId, int targetId, Action onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            onSuccess?.Invoke();
        }));
    }

    public void GetPendingRequests(int playerId, Action<FriendRequest[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            var fakeRequests = new FriendRequest[]
            {
                new FriendRequest { id = 101, pseudo = "mama", requestedAt = "2025-07-28" },
                new FriendRequest { id = 102, pseudo = "juju", requestedAt = "2025-07-27" }
            };
            onSuccess?.Invoke(fakeRequests);
        }));
    }

    public void RespondFriendRequest(int playerId, int requesterId, string action, Action onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            onSuccess?.Invoke();
        }));
    }

    public void ToggleFavorite(int playerId, int targetId, bool isFavorite, Action<bool> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            onSuccess?.Invoke(isFavorite);
        }));
    }

    public void SearchUsers(int playerId, string searchText, Action<UserInfo[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            var fakeUsers = new UserInfo[]
            {
                new UserInfo { id = 10, pseudo = "maria" },
                new UserInfo { id = 11, pseudo = "sora" }
            };
            onSuccess?.Invoke(fakeUsers);
        }));
    }

    public void GetOnlineFriends(int playerId, Action<int[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            var onlineIds = new int[] { 2, 3 };
            onSuccess?.Invoke(onlineIds);
        }));
    }

    public void GetBlockedFriends(int playerId, Action<Friend[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            var fakeBlocked = new Friend[]
            {
                new Friend { id = 4, pseudo = "Dave", avatar = "avatar4", is_favorite = false },
                new Friend { id = 5, pseudo = "Eve",  avatar = "avatar5", is_favorite = false }
            };
            onSuccess?.Invoke(fakeBlocked);
        }));
    }

    public void UpdateAvatar(int playerId, string avatarName, Action onSuccess, Action<string> onError)
    {
        StartCoroutine(MockDelay(() =>
        {
            onSuccess?.Invoke();
        }));
    }

    private IEnumerator MockDelay(Action callback)
    {
        yield return new WaitForSeconds(0.3f);
        callback?.Invoke();
    }
}
