using UnityEngine;
using TMPro;

[System.Serializable]
public class PlayerProfessions
{
    public int pecheurLevel = 1;
    public int paysanLevel = 1;
    public int mineurLevel = 1;
    public int bucheronLevel = 1;
}

public class PlayerGUIBinder : MonoBehaviour
{
    [Header("Références Joueur")]
    public Entity_Info playerStats;
    public PlayerProfessions playerProfessions;

    [Header("UI – TextMeshPro")]
    public TMP_Text lvPlayerText;
    public TMP_Text lvPecheurText;
    public TMP_Text lvPaysanText;
    public TMP_Text lvMineurText;
    public TMP_Text lvBucheronText;

    void Update()
    {
        if (playerStats == null || playerProfessions == null) return;

        lvPlayerText.text = $"{playerStats.entity_Level}";
        lvPecheurText.text = $"{playerProfessions.pecheurLevel}";
        lvPaysanText.text = $"{playerProfessions.paysanLevel}";
        lvMineurText.text = $"{playerProfessions.mineurLevel}";
        lvBucheronText.text = $"{playerProfessions.bucheronLevel}";
    }
}
