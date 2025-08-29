using UnityEngine;
using TMPro;
using System.Globalization;

public class GoldDisplay : MonoBehaviour
{
    [Header("Référence UI")]
    [SerializeField] private TMP_Text goldText;

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();

        if (goldText == null)
        {
            Debug.LogError("Aucun TMP_Text assigné dans l'inspecteur pour GoldDisplay !");
        }
    }

    private void Update()
    {
        if (playerStats != null && goldText != null)
        {
            goldText.text = playerStats.gold.ToString("N0", new CultureInfo("de-DE"));
        }
    }
}
