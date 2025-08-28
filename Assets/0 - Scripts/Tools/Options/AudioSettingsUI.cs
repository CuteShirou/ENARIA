using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI")]
    public Toggle soundToggle;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource[] sfxSources;

    private float musicVolume = 0.8f;
    private float sfxVolume = 0.8f;
    private float masterVolume = 1f;

    void Start()
    {
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        AudioListener.pause = !soundEnabled;
        soundToggle.isOn = soundEnabled;

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        masterSlider.value = masterVolume;

        ApplyVolumes();

        soundToggle.onValueChanged.AddListener(OnToggleSound);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
    }

    void OnToggleSound(bool isOn)
    {
        AudioListener.pause = !isOn;
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
    }

    void OnMusicSliderChanged(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyVolumes();
    }

    void OnSFXSliderChanged(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        ApplyVolumes();
    }

    void OnMasterSliderChanged(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;

        foreach (var sfx in sfxSources)
        {
            sfx.volume = sfxVolume * masterVolume;
        }
    }
}
