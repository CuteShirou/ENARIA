using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHP = 200;
    public int maxPA = 7;
    public int maxPM = 4;

    [Header("Current Stats")]
    public int currentHP;
    public int currentPA;
    public int currentPM;

    private void Awake()
    {
        currentHP = maxHP;
        currentPA = maxPA;
        currentPM = maxPM;
    }

    public void ResetTurnStats()
    {
        currentPA = maxPA;
        currentPM = maxPM;
    }
}
