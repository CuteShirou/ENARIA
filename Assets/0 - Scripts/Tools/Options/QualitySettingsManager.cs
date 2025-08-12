using UnityEngine;

public class QualitySettingsManager : MonoBehaviour
{
    public void SetQualityLow()
    {
        QualitySettings.SetQualityLevel(0);
        PlayerPrefs.SetInt("QualityLevel", 0);
    }

    public void SetQualityMedium()
    {
        QualitySettings.SetQualityLevel(2);
        PlayerPrefs.SetInt("QualityLevel", 2);
    }

    public void SetQualityHigh()
    {
        QualitySettings.SetQualityLevel(5);
        PlayerPrefs.SetInt("QualityLevel", 5);
    }

    void Start()
    {
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(quality);
    }
}
