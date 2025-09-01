using UnityEngine;

public class ScreenModeManager : MonoBehaviour
{
    public void SetFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerPrefs.SetInt("Fullscreen", 1);
    }

    public void SetWindowed()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", 0);
    }

    void Start()
    {
        int fullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        Screen.fullScreenMode = fullscreen == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
}
