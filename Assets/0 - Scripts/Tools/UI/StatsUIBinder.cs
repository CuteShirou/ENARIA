using UnityEngine;
using TMPro;

public class StatsUIBinder : MonoBehaviour
{
    private int[] allocatedPoints = new int[5];

    [Header("Références")]
    public PlayerStats playerStats;
    public CombatStats combatStats;
    public EquipmentManager equipmentManager;
    public GameObject detailedStatsPanel;

    [Header("Informations Joueur")]
    public TextMeshProUGUI pseudoText;
    public TextMeshProUGUI specieText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI remainingPoints;
    public TextMeshProUGUI experienceText;

    [Header("Sources")]
    public TMP_InputField pointInputField;
    public GameObject[] allocationButtons;

    [Header("Colonnes – Stats Principales")]
    public TextMeshProUGUI[] equipmentColumn;
    public TextMeshProUGUI[] levelUpColumn;
    public TextMeshProUGUI[] totalColumn;

    [Header("Colonnes – Stats détaillées")]
    public TextMeshProUGUI[] detailedEquipColumn;
    public TextMeshProUGUI[] detailedBaseColumn;
    // public TextMeshProUGUI[] detailedTempColumn;
    public TextMeshProUGUI[] detailedTotalColumn;

    void Start()
    {
        if (combatStats == null || playerStats == null)
        {
            Debug.LogError("CombatStats ou PlayerStats non assigné !");
            return;
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshMainStats();
        RefreshDetailedStats();
        UpdateAllocationButtonsVisibility();
    }

    private void RefreshMainStats()
    {
        int[] baseValues = new int[]
        {
            combatStats.baseHP,
            combatStats.basePA,
            combatStats.basePM,
            combatStats.basePO,
            combatStats.baseForce,
            combatStats.baseDexterite,
            combatStats.baseMagie,
            combatStats.baseFoi
        };

        float[] equipValues = GetEquipmentBonuses();

        for (int i = 0; i < baseValues.Length; i++)
        {
            equipmentColumn[i].text = Mathf.RoundToInt(equipValues[i]).ToString();

            int addedFromPoints = 0;
            switch (i)
            {
                case 0: addedFromPoints = allocatedPoints[0]; break; // PV
                case 4: addedFromPoints = allocatedPoints[1]; break; // Force
                case 5: addedFromPoints = allocatedPoints[2]; break; // Dex
                case 6: addedFromPoints = allocatedPoints[3]; break; // Magie
                case 7: addedFromPoints = allocatedPoints[4]; break; // Foi
            }

            levelUpColumn[i].text = addedFromPoints.ToString();

            float total = baseValues[i] + equipValues[i] + addedFromPoints;
            totalColumn[i].text = Mathf.RoundToInt(total).ToString();
        }

        pseudoText.text = playerStats.pseudo;
        specieText.text = playerStats.specie.ToString();
        levelText.text = "Lv " + playerStats.level;
        remainingPoints.text = "Points restants : " + playerStats.remainingPoints;
        experienceText.text = $"EXP : {playerStats.experience} / {playerStats.ExperienceToNextLevel}";
    }

    private void RefreshDetailedStats()
    {
        float[] baseDetailed = new float[]
        {
            combatStats.baseInitiative,
            combatStats.baseCritChance,
            combatStats.baseResistanceForce,
            combatStats.baseResistanceDexterite,
            combatStats.baseResistanceMagie,
            combatStats.baseResistanceFoi
        };

        float[] equipment = GetEquipmentBonuses();
        // float[] tempBonuses = GetTemporaryBonuses();

        for (int i = 0; i < 6; i++)
        {
            float equip = equipment[i + 8];
            float baseVal = baseDetailed[i];
            // float temp = tempBonuses[i];
            float total = baseVal + equip; // + temp;

            bool isPercentStat = (i == 1 || i >= 2); // Crit & Resistances

            string Format(float val) => isPercentStat ? val.ToString("0.##") + "%" : val.ToString("0.##");

            detailedBaseColumn[i].text = Format(baseVal);
            detailedEquipColumn[i].text = Format(equip);
            // detailedTempColumn[i].text = Format(temp);
            detailedTotalColumn[i].text = Format(total);
        }
    }

    public void TryAllocatePoint(StatAllocationButton.StatType stat)
    {
        if (playerStats.remainingPoints <= 0)
            return;

        int requested = 1;
        if (pointInputField != null)
        {
            if (!int.TryParse(pointInputField.text, out requested))
                requested = 1;
        }

        requested = Mathf.Max(1, requested);
        int toAllocate = Mathf.Min(requested, playerStats.remainingPoints);

        int index = (int)stat;
        allocatedPoints[index] += toAllocate;
        playerStats.remainingPoints -= toAllocate;

        RefreshAll();
    }

    private float[] GetEquipmentBonuses()
    {
        if (equipmentManager != null)
            return equipmentManager.GetEquipmentBonuses();

        return new float[13];
    }

    //private float[] GetTemporaryBonuses()
    //{
    //    float[] bonuses = new float[6];

    //    foreach (var effect in combatStats.activeEffects)
    //    {
    //        if (effect.turnsRemaining <= 0) continue;

    //        var val = effect.effect.value;
    //        switch (effect.effect.effectType)
    //        {
    //            case EffectType.BonusInitiative: bonuses[0] += val; break;
    //            case EffectType.BonusCritChance: bonuses[1] += val; break;
    //            case EffectType.BonusResFor: bonuses[2] += val; break;
    //            case EffectType.BonusResDex: bonuses[3] += val; break;
    //            case EffectType.BonusResMag: bonuses[4] += val; break;
    //            case EffectType.BonusResFoi: bonuses[5] += val; break;
    //        }
    //    }

    //    return bonuses;
    //}

    private void UpdateAllocationButtonsVisibility()
    {
        bool show = playerStats.remainingPoints > 0;

        foreach (var button in allocationButtons)
        {
            button.SetActive(show);
        }
    }

    public void ToggleDetailedStatsPanel()
    {
        if (detailedStatsPanel == null) return;

        bool isActive = detailedStatsPanel.activeSelf;
        detailedStatsPanel.SetActive(!isActive);
    }
}
