using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class QuestSaver : MonoBehaviour
{
    public string saveQuestURL = "https://enaria.nexus-com.fr/save_quest.php";

    private string httpUsername = "enaria@nexus-com.fr";
    private string httpPassword = "EnariaAdmin";

    public void SaveQuestToServer(string playerId, string questId, int isAccepted, int isCompleted, int currentStepIndex, int stepProgress)
    {
        StartCoroutine(SendQuest(playerId, questId, isAccepted, isCompleted, currentStepIndex, stepProgress));
    }

    private IEnumerator SendQuest(string playerId, string questId, int isAccepted, int isCompleted, int currentStepIndex, int stepProgress)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", playerId);
        form.AddField("quest_id", questId);
        form.AddField("is_accepted", isAccepted);
        form.AddField("is_completed", isCompleted);
        form.AddField("current_step_index", currentStepIndex);
        form.AddField("step_progress", stepProgress);

        using UnityWebRequest www = UnityWebRequest.Post(saveQuestURL, form);

        string auth = httpUsername + ":" + httpPassword;
        string encodedAuth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        www.SetRequestHeader("Authorization", "Basic " + encodedAuth);

        yield return www.SendWebRequest();

        Debug.Log($"HTTP Status Code: {www.responseCode}");
        Debug.Log("Réponse brute serveur : " + www.downloadHandler.text);

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("<color=green>Succès :</color> " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Erreur réseau ({www.error}) - HTTP Code: {www.responseCode}");
        }
    }

}
