using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelBarUI : MonoBehaviour
{
    [Header("Références principales")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Image xpFillBar; 
    [SerializeField] private TMP_Text xpText;

    [Header("Barre de vie")]
    [SerializeField] private Image hpFillBar;
    [SerializeField] private TMP_Text hpText;

    [Header("Source de HP (optionnel)")]
    [Tooltip("Si tu as un component qui expose les champs/propriétés currentHP / maxHP, glisse-le ici.")]
    [SerializeField] private MonoBehaviour hpProvider;
    [Tooltip("Nom du champ/propriété 'current' sur hpProvider (ex: currentHP).")]
    [SerializeField] private string hpCurrentName = "currentHP";
    [Tooltip("Nom du champ/propriété 'max' sur hpProvider (ex: maxHP).")]
    [SerializeField] private string hpMaxName = "maxHP";

    [Header("Fallback manuel (si pas de provider)")]
    [SerializeField] private bool useManualHP = false;
    [SerializeField] private int manualCurrentHP = 100;
    [SerializeField] private int manualMaxHP = 100;

    private void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void Update()
    {
        UpdateXPBar();
        UpdateHPBar();
    }

    private void UpdateXPBar()
    {
        if (playerStats == null || xpFillBar == null) return;

        int xp = playerStats.experience;
        int xpToNext = playerStats.ExperienceToNextLevel;
        float ratio = xpToNext > 0 ? (float)xp / xpToNext : 0f;
        xpFillBar.fillAmount = Mathf.Clamp01(ratio);

        if (xpText != null)
            xpText.text = $"{xp} / {xpToNext}";
    }

    private void UpdateHPBar()
    {
        if (hpFillBar == null) return;

        int current = 0;
        int max = 0;
        bool got = false;

        if (hpProvider != null)
        {
            (got, current, max) = ReadIntsFromProvider(hpProvider, hpCurrentName, hpMaxName);
        }

        if (!got && playerStats != null)
        {
            (got, current, max) = ReadIntsFromProvider(playerStats, "currentHP", "maxHP");
        }

        if (!got && useManualHP)
        {
            current = manualCurrentHP;
            max = manualMaxHP;
            got = true;
        }

        if (!got)
        {
            hpFillBar.fillAmount = 0f;
            if (hpText != null) hpText.text = "-";
            return;
        }

        max = Mathf.Max(1, max);
        current = Mathf.Clamp(current, 0, max);
        hpFillBar.fillAmount = (float)current / max;

        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }

    private (bool, int, int) ReadIntsFromProvider(object provider, string curName, string maxName)
    {
        if (provider == null) return (false, 0, 0);

        Type t = provider.GetType();

        bool okCur = TryGetIntMember(t, provider, curName, out int curVal);
        bool okMax = TryGetIntMember(t, provider, maxName, out int maxVal);

        if (okCur && okMax) return (true, curVal, maxVal);

        return (false, 0, 0);
    }

    private bool TryGetIntMember(Type t, object instance, string name, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(name)) return false;

        var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long)))
        {
            try
            {
                object v = prop.GetValue(instance);
                value = Convert.ToInt32(v);
                return true;
            }
            catch { return false; }
        }

        var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && (field.FieldType == typeof(int) || field.FieldType == typeof(long)))
        {
            try
            {
                object v = field.GetValue(instance);
                value = Convert.ToInt32(v);
                return true;
            }
            catch { return false; }
        }

        return false;
    }

    public void SetManualHP(int current, int max)
    {
        manualCurrentHP = current;
        manualMaxHP = max;
    }

    public void SetUseManualHP(bool use)
    {
        useManualHP = use;
    }
}
