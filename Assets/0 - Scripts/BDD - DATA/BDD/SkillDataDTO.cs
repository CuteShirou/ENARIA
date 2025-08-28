using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

[Serializable]
public class SkillDataDTO
{
    public int id;
    public string name;
    public string description;
    public int degat_min;
    public int degat_max;
    public string type_elementaire;
    public int cout_pa;
    public int portee_min;
    public int portee_max;
    public int cooldown;
    public int max_per_target_per_turn;
    public float crit_percent;
    public string case_impact;
    public string effet_basique;
    public string effet_bonus_crit;
}

[Serializable]
public class SkillsResponse
{
    public SkillDataDTO[] items;
}

[Serializable]
public class UnlockResponse
{
    public bool success;
}

public class SkillApi : MonoBehaviour
{
    [Header("API Settings")]
    public string baseUrl = "https://enaria.nexus-com.fr/player_skill.php";
    public string httpUsername = "enaria@nexus-com.fr";
    public string httpPassword = "EnariaAdmin";

    public void FetchUnlockedSkills(string playerId, Action<SkillDataDTO[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(FetchCoroutine(playerId, onSuccess, onError));
    }

    private IEnumerator FetchCoroutine(string playerId, Action<SkillDataDTO[]> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}?action=get&player_id={UnityWebRequest.EscapeURL(playerId)}";
        using UnityWebRequest www = UnityWebRequest.Get(url);
        AddBasicAuth(www);

        yield return www.SendWebRequest();

        Debug.Log($"[Skills GET] HTTP {www.responseCode}");
        if (www.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(www.error);
            yield break;
        }

        try
        {
            SkillsResponse resp = JsonUtility.FromJson<SkillsResponse>(www.downloadHandler.text);
            onSuccess?.Invoke(resp.items);
        }
        catch (Exception ex)
        {
            onError?.Invoke("JSON parse error: " + ex.Message);
        }
    }

    public void UnlockSkill(string playerId, string skillId, Action<bool> onComplete)
    {
        StartCoroutine(UnlockCoroutine(playerId, skillId, onComplete));
    }

    private IEnumerator UnlockCoroutine(string playerId, string skillId, Action<bool> onComplete)
    {
        WWWForm form = new WWWForm();
        form.AddField("action", "unlock");
        form.AddField("player_id", playerId);
        form.AddField("skill_id", skillId);

        using UnityWebRequest www = UnityWebRequest.Post(baseUrl, form);
        AddBasicAuth(www);

        yield return www.SendWebRequest();

        Debug.Log($"[Skills POST] HTTP {www.responseCode}");
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            onComplete?.Invoke(false);
            yield break;
        }

        try
        {
            UnlockResponse resp = JsonUtility.FromJson<UnlockResponse>(www.downloadHandler.text);
            onComplete?.Invoke(resp.success);
        }
        catch (Exception ex)
        {
            Debug.LogError("JSON parse error: " + ex.Message);
            onComplete?.Invoke(false);
        }
    }

    private void AddBasicAuth(UnityWebRequest www)
    {
        string credentials = $"{httpUsername}:{httpPassword}";
        string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        www.SetRequestHeader("Authorization", "Basic " + encoded);
    }
}
