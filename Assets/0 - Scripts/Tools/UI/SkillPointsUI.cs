using UnityEngine;
using TMPro;

public class SkillPointsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private SkillTreeManager skillTreeManager;

    private void Start()
    {
        if (skillTreeManager == null)
            skillTreeManager = FindObjectOfType<SkillTreeManager>();

        UpdatePointsDisplay();
    }

    private void OnEnable()
    {
        UpdatePointsDisplay();
    }

    public void UpdatePointsDisplay()
    {
        if (pointsText != null && skillTreeManager != null)
        {
            pointsText.text = $"Points restants : {skillTreeManager.availablePoints}";
        }
    }
}
