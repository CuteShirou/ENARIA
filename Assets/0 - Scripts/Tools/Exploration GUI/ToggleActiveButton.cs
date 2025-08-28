using UnityEngine;
using UnityEngine.UI;

public class ToggleActiveButton : MonoBehaviour
{
    [Tooltip("L'objet à afficher/cacher")]
    public GameObject target;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ToggleTarget);
        }
        else
        {
            Debug.LogWarning("ToggleActiveButton doit être attaché à un Button !");
        }
    }

    private void ToggleTarget()
    {
        if (target == null)
        {
            Debug.LogWarning("ToggleActiveButton : target non assigné !");
            return;
        }

        target.SetActive(!target.activeSelf);
    }
}
