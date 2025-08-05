using UnityEngine;
using UnityEngine.UI;
using static StatAllocationButton;

public class StatAllocationButton : MonoBehaviour
{
    public enum StatType
    {
        PV, FOR, DEX, MAG, FOI
    }

    [Header("Quelle stat augmenter")]
    public StatType statType;

    [Header("Référence au binder")]
    public StatsUIBinder statsUIBinder;

    public void OnClick()
    {
        statsUIBinder.TryAllocatePoint(statType);
    }
}
