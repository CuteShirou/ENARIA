using UnityEngine;
using TMPro;

public class StatsUIBinder : MonoBehaviour
{
    // Tableau interne pour retenir les points alloués temporairement dans l'UI
    private int[] allocatedPoints = new int[5];

    [Header("Références")]
    public Entity_Info playerStats;                    // Référence vers les infos persos du joueur (niveau, xp, points restants)
    public Entity_StatistiqueCombat combatStats;       // Référence vers les stats de combat (base, résistances, etc.)
    public EquipmentManager equipmentManager;          // Référence vers le gestionnaire d'équipement
    public GameObject detailedStatsPanel;              // Panneau des stats détaillées

    [Header("Informations Joueur")]
    public TextMeshProUGUI pseudoText;                 // Affichage du pseudo
    public TextMeshProUGUI specieText;                 // Affichage de l'espèce
    public TextMeshProUGUI levelText;                  // Affichage du niveau
    public TextMeshProUGUI remainingPoints;            // Affichage des points restants
    public TextMeshProUGUI experienceText;             // Affichage de l'xp courante

    [Header("Sources")]
    public TMP_InputField pointInputField;             // Input pour allouer plusieurs points d'un coup
    public GameObject[] allocationButtons;             // Boutons d'allocation visibles si points restants > 0

    [Header("Colonnes – Stats Principales")]
    public TextMeshProUGUI[] equipmentColumn;          // Colonne bonus équipement (PV, PA, PM, PO, For, Dex, Mag, Foi)
    public TextMeshProUGUI[] levelUpColumn;            // Colonne bonus via points alloués
    public TextMeshProUGUI[] totalColumn;              // Colonne total (base + équipement + points)

    [Header("Colonnes – Stats détaillées")]
    public TextMeshProUGUI[] detailedEquipColumn;      // Bonus équipement (Initiative, Crit, Res For/Dex/Mag/Foi)
    public TextMeshProUGUI[] detailedBaseColumn;       // Valeur de base
    // public TextMeshProUGUI[] detailedTempColumn;    // Bonus temporaires (désactivé pour le moment)
    public TextMeshProUGUI[] detailedTotalColumn;      // Total détaillé

    // ---------------------------------------------------------
    // Start : vérifie les références et fait un premier rafraîchissement
    private void Start()
    {
        if (combatStats == null || playerStats == null)
        {
            Debug.LogError("CombatStats ou PlayerStats non assigné !");
            return;
        }

        RefreshAll();
    }

    // ---------------------------------------------------------
    // OnEnable : déclenché à chaque fois que le GameObject STATSUI est activé
    //   Permet d'actualiser l'affichage quand on ouvre l'onglet (après un gain d'XP, level-up, etc.)
    private void OnEnable()
    {
        SafeRefreshIfReady();
    }

    // ---------------------------------------------------------
    // SafeRefreshIfReady : rafraîchit uniquement si les références sont valides
    private void SafeRefreshIfReady()
    {
        if (playerStats != null && combatStats != null)
        {
            RefreshAll();
        }
    }

    // ---------------------------------------------------------
    // RefreshAll : met à jour tous les blocs d'affichage (principales + détaillées + visibilité des boutons)
    public void RefreshAll()
    {
        RefreshMainStats();
        RefreshDetailedStats();
        UpdateAllocationButtonsVisibility();
    }

    // ---------------------------------------------------------
    // RefreshMainStats : met à jour l'entête joueur et les 3 colonnes des stats principales
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
            // Bonus équipement arrondi
            equipmentColumn[i].text = Mathf.RoundToInt(equipValues[i]).ToString();

            // Bonus via points alloués depuis l'UI (valeurs "en attente")
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

            // Total = base + équipement + points alloués (en attente si on n'a pas encore "appliqué")
            float total = baseValues[i] + equipValues[i] + addedFromPoints;
            totalColumn[i].text = Mathf.RoundToInt(total).ToString();
        }

        // En-tête joueur
        pseudoText.text = playerStats.entity_Name;
        specieText.text = playerStats.specie.ToString();
        levelText.text = "Lv " + playerStats.entity_Level;
        remainingPoints.text = "Points restants : " + playerStats.remainingPoints;
        experienceText.text = $"EXP : {playerStats.experience} / {playerStats.GetExperienceToNextLevel()}";
    }

    // ---------------------------------------------------------
    // RefreshDetailedStats : met à jour les stats avancées (initiative, crit, résistances)
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

            bool isPercentStat = (i == 1 || i >= 2); // Crit & Résistances

            string Format(float val) => isPercentStat ? val.ToString("0.##") + "%" : val.ToString("0.##");

            detailedBaseColumn[i].text = Format(baseVal);
            detailedEquipColumn[i].text = Format(equip);
            // detailedTempColumn[i].text = Format(temp);
            detailedTotalColumn[i].text = Format(total);
        }
    }

    // ---------------------------------------------------------
    // TryAllocatePoint : tente d'allouer des points via l'UI, applique dans les stats, puis rafraîchit
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
        if (toAllocate <= 0) return;

        int index = (int)stat;

        //   1) Mémorise côté UI (affichage "LevelUp" temporaire)
        allocatedPoints[index] += toAllocate;

        //   2) Décrémente les points restants du joueur
        playerStats.remainingPoints -= toAllocate;

        //   3) Applique IMMÉDIATEMENT et de façon PERMANENTE dans les stats (base + current)
        ApplyPermanentStatIncrease(stat, toAllocate);

        //   4) On remet la colonne "LevelUp" à 0 (on a déjà appliqué en base, éviter le double-count)
        allocatedPoints[index] = 0;

        //   5) Rafraîchit l'affichage
        RefreshAll();
    }

    // ---------------------------------------------------------
    // ApplyPermanentStatIncrease : applique l'augmentation dans Entity_StatistiqueCombat
    private void ApplyPermanentStatIncrease(StatAllocationButton.StatType stat, int amount)
    {
        //   Ici on appelle des helpers ajoutés dans Entity_StatistiqueCombat
        switch (stat)
        {
            case StatAllocationButton.StatType.PV:
                combatStats.AddBaseHP(amount, true);      // true : on rend aussi les PV pour refléter l'augmentation de max
                break;
            case StatAllocationButton.StatType.FOR:
                combatStats.AddBaseForce(amount);
                break;
            case StatAllocationButton.StatType.DEX:
                combatStats.AddBaseDex(amount);
                break;
            case StatAllocationButton.StatType.MAG:
                combatStats.AddBaseMagie(amount);
                break;
            case StatAllocationButton.StatType.FOI:
                combatStats.AddBaseFoi(amount);
                break;
        }
    }

    // ---------------------------------------------------------
    // GetEquipmentBonuses : retourne les bonus équipement attendus par l'UI
    private float[] GetEquipmentBonuses()
    {
        if (equipmentManager != null)
            return equipmentManager.GetEquipmentBonuses();

        return new float[13];
    }

    //// ---------------------------------------------------------
    //// GetTemporaryBonuses : exemple de calcul de bonus temporaires (désactivé)
    //private float[] GetTemporaryBonuses()
    //{
    //    float[] bonuses = new float[6];
    //
    //    foreach (var effect in combatStats.activeEffects)
    //    {
    //        if (effect.turnsRemaining <= 0) continue;
    //
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
    //
    //    return bonuses;
    //}

    // ---------------------------------------------------------
    // UpdateAllocationButtonsVisibility : affiche/masque les boutons d'allocation selon les points restants
    private void UpdateAllocationButtonsVisibility()
    {
        bool show = playerStats.remainingPoints > 0;

        foreach (var button in allocationButtons)
        {
            button.SetActive(show);
        }
    }

    // ---------------------------------------------------------
    // ToggleDetailedStatsPanel : ouvre/ferme le panneau de stats détaillées
    public void ToggleDetailedStatsPanel()
    {
        if (detailedStatsPanel == null) return;

        bool isActive = detailedStatsPanel.activeSelf;
        detailedStatsPanel.SetActive(!isActive);
    }
}
