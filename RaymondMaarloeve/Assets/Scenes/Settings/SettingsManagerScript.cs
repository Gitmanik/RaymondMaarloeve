using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown fullscreenDropdown; // ← zmienione z Toggle na TMP_Dropdown
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    private void Start()
    {
        LoadSettings();
    }

    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
        Debug.Log("Saved GraphicsQuality: " + index);
    }

    public void SetFullscreen(int dropdownIndex)
    {
        // 0 = Fullscreen, 1 = Windowed
        bool isFullscreen = (dropdownIndex == 0);
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Saved Fullscreen: " + isFullscreen);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        Debug.Log("Saved MasterVolume: " + volume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        Debug.Log("Saved MusicVolume: " + volume);
    }

    public void LoadSettings()
    {
        int graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);

        graphicsDropdown.value = graphicsQuality;
        fullscreenDropdown.value = isFullscreen ? 0 : 1;
        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;

        SetGraphicsQuality(graphicsQuality);
        SetFullscreen(fullscreenDropdown.value);
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);

        Debug.Log("Loaded settings from PlayerPrefs");
        PrintPrefs();
    }

    public void ResetToDefaults()
    {
        graphicsDropdown.value = 2;
        fullscreenDropdown.value = 0; // Fullscreen
        masterVolumeSlider.value = 1.0f;
        musicVolumeSlider.value = 1.0f;

        SetGraphicsQuality(2);
        SetFullscreen(0);
        SetMasterVolume(1.0f);
        SetMusicVolume(1.0f);

        Debug.Log("Settings reset to defaults");
        PrintPrefs();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PrintPrefs()
    {
        Debug.Log("=== Current Saved Settings ===");
        Debug.Log("GraphicsQuality: " + PlayerPrefs.GetInt("GraphicsQuality", -1));
        Debug.Log("Fullscreen: " + PlayerPrefs.GetInt("Fullscreen", -1));
        Debug.Log("MasterVolume: " + PlayerPrefs.GetFloat("MasterVolume", -1));
        Debug.Log("MusicVolume: " + PlayerPrefs.GetFloat("MusicVolume", -1));
        Debug.Log("==============================");
    }
}
