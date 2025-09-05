using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelBarUI : MonoBehaviour
{
    [Header("Références principales")]
    [SerializeField] private Entity_Info playerStats;                  // Référence obligatoire pour la barre d'XP
    [SerializeField] private Entity_StatistiqueCombat combatStats;     // Référence obligatoire pour la barre de HP

    [Header("Barre d'XP")]
    [SerializeField] private Image xpFillBar;
    [SerializeField] private TMP_Text xpText;

    [Header("Barre de vie")]
    [SerializeField] private Image hpFillBar;
    [SerializeField] private TMP_Text hpText;

    [Header("Options d'initialisation HP")]
    [Tooltip("Si actif, à l'ouverture de l'UI et si les HP semblent non initialisés, on appelle InitStatsFromBase().")]
    [SerializeField] private bool autoInitHPOnEnable = true;

    // ---------------------------------------------------------
    // OnEnable : déclenché quand l'objet UI est activé (au lancement et à chaque ouverture)
    private void OnEnable()
    {
        // Sécurise l'init HP uniquement si nécessaire (évite la barre vide au tout début)
        TryAutoInitHPOnce();

        // Rafraîchit immédiatement les deux barres à l'ouverture
        UpdateXPBar();
        UpdateHPBar();
    }

    // ---------------------------------------------------------
    // Update : met à jour en continu l'XP et les HP (si tu veux, on pourra passer en modèle à évènements)
    private void Update()
    {
        UpdateXPBar();
        UpdateHPBar();
    }

    // ---------------------------------------------------------
    // LinkTargets : permet de lier les références par code, sans utiliser de Find()
    public void LinkTargets(Entity_Info info, Entity_StatistiqueCombat stats)
    {
        // Met à jour les références utilisées par l'UI
        playerStats = info;
        combatStats = stats;
    }

    // ---------------------------------------------------------
    // UpdateXPBar : met à jour la barre d'expérience
    private void UpdateXPBar()
    {
        if (playerStats == null || xpFillBar == null) return;

        int xp = playerStats.experience;
        int xpToNext = playerStats.GetExperienceToNextLevel();

        float ratio = xpToNext > 0 ? (float)xp / xpToNext : 0f;
        xpFillBar.fillAmount = Mathf.Clamp01(ratio);

        if (xpText != null)
            xpText.text = $"{xp} / {xpToNext}";
    }

    // ---------------------------------------------------------
    // UpdateHPBar : lit directement currentHP / baseHP depuis Entity_StatistiqueCombat
    private void UpdateHPBar()
    {
        if (combatStats == null || hpFillBar == null) return;

        int max = Mathf.Max(1, combatStats.baseHP);
        int current = Mathf.Clamp(combatStats.currentHP, 0, max);

        hpFillBar.fillAmount = (float)current / max;

        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }

    // ---------------------------------------------------------
    // TryAutoInitHPOnce : initialise les HP si on détecte un état non initialisé au moment d'ouvrir l'UI
    private void TryAutoInitHPOnce()
    {
        // On ne tente rien si l'option est désactivée ou si les refs sont manquantes
        if (!autoInitHPOnEnable || combatStats == null) return;

        // On évite de forcer une init si on a déjà des HP valides ou si le contexte ne s'y prête pas
        if (!ShouldAutoInitHP()) return;

        // Appelle la méthode prévue par Entity_StatistiqueCombat pour copier les bases vers les current
        combatStats.InitStatsFromBase();
    }

    // ---------------------------------------------------------
    // ShouldAutoInitHP : renvoie true si les HP semblent non initialisés et que l'entité peut être init
    private bool ShouldAutoInitHP()
    {
        // Conditions:
        // - baseHP > 0
        // - currentHP <= 0 (typiquement 0 au tout début si aucune init)
        // - pas en combat (on n'override pas une logique de combat en cours)
        // - pas morte (ne pas "réanimer" visuellement un mort)
        if (combatStats.baseHP <= 0) return false;
        if (combatStats.currentHP > 0) return false;
        if (combatStats.isFight) return false;
        if (combatStats.isDead) return false;

        return true;
    }
}
