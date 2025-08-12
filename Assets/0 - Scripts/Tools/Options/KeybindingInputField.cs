using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeybindingInputField : MonoBehaviour
{
    public string actionName;
    public TMP_InputField inputField;

    private bool waitingForKey = false;

    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        LoadKey();
    }

    void LoadKey()
    {
        string savedKey = PlayerPrefs.GetString(actionName, KeyCode.None.ToString());
        inputField.text = savedKey;
    }

    public void OnSelect()
    {
        waitingForKey = true;
        inputField.text = "...";
    }

    void Update()
    {
        if (waitingForKey)
        {
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    PlayerPrefs.SetString(actionName, kcode.ToString());
                    inputField.text = kcode.ToString();
                    waitingForKey = false;

                    inputField.DeactivateInputField();

                    break;
                }
            }
        }
    }
}
