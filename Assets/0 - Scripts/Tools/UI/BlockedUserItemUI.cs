using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlockedUserItemUI : MonoBehaviour
{
    public TMP_Text usernameTMP;
    public Button unblockButton;

    public void Setup(string username, System.Action onUnblock)
    {
        if (usernameTMP != null)
            usernameTMP.text = username;

        unblockButton.onClick.RemoveAllListeners();
        unblockButton.onClick.AddListener(() => onUnblock?.Invoke());
    }
}
