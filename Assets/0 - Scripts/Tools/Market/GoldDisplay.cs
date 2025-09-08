using UnityEngine;
using TMPro;
using System.Globalization;

public class GoldDisplay : MonoBehaviour
{
    [Header("Référence UI")]
    [SerializeField] private TMP_Text goldText;

    [Header("Sources possibles")]
    [SerializeField] private Entity_Info entityInfo;

    private void Awake()
    {
        if (entityInfo == null) entityInfo = FindObjectOfType<Entity_Info>();
    }

    private void Update()
    {
        if (goldText == null) return;

        long goldValue = 0;

        if (entityInfo != null)
            goldValue = entityInfo.gold;

        goldText.text = goldValue.ToString("N0", new CultureInfo("de-DE"));
    }
}
