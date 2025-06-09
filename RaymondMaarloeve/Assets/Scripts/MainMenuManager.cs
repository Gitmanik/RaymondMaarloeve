using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the main menu and settings UI, including scene transitions,
/// graphics and audio settings, and PlayerPrefs persistence.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    /// <summary>
    /// Dropdown for selecting graphics quality level.
    /// </summary>
    public TMP_Dropdown graphicsDropdown;

    /// <summary>
    /// Dropdown for selecting fullscreen mode (0 = fullscreen, 1 = windowed).
    /// </summary>
    public TMP_Dropdown fullscreenDropdown;

    /// <summary>
    /// Slider for controlling the master audio volume.
    /// </summary>
    public Slider masterVolumeSlider;

    /// <summary>
    /// Slider for controlling the music volume.
    /// </summary>
    public Slider musicVolumeSlider;

    [Header("Scene Names")]
    /// <summary>
    /// Name of the scene that serves as a loading screen or transition to gameplay.
    /// </summary>
    public string LoadingscreenSceneName = "LoadingScene";

    /// <summary>
    /// Name of the main menu scene.
    /// </summary>
    public string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Name of the settings menu scene.
    /// </summary>
    public string settingsSceneName = "Settings";

    /// <summary>
    /// Initializes settings when entering the settings scene.
    /// </summary>
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == settingsSceneName)
            LoadSettings();
    }

    // === Menu Buttons ===

    /// <summary>
    /// Starts the game by loading the loading screen or game scene.
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(LoadingscreenSceneName);
    }

    /// <summary>
    /// Quits the application or stops play mode if in the Unity Editor.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit button pressed.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Loads the settings scene from the main menu.
    /// </summary>
    public void LoadSettingsScene()
    {
        SceneManager.LoadScene(settingsSceneName);
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // === Settings Functions ===

    /// <summary>
    /// Sets the graphics quality based on dropdown index and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="index">Index of the graphics quality level.</param>
    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets fullscreen or windowed mode based on dropdown index and saves it.
    /// </summary>
    /// <param name="dropdownIndex">0 for fullscreen, 1 for windowed.</param>
    public void SetFullscreen(int dropdownIndex)
    {
        bool isFullscreen = (dropdownIndex == 0);
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets the master audio volume and saves it.
    /// </summary>
    /// <param name="volume">Volume level between 0.0 and 1.0.</param>
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets the music volume and saves it.
    /// </summary>
    /// <param name="volume">Music volume level between 0.0 and 1.0.</param>
    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads settings from PlayerPrefs and applies them to the UI and game.
    /// </summary>
    public void LoadSettings()
    {
        int graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);

        if (graphicsDropdown != null) graphicsDropdown.value = graphicsQuality;
        if (fullscreenDropdown != null) fullscreenDropdown.value = isFullscreen ? 0 : 1;
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;

        SetGraphicsQuality(graphicsQuality);
        SetFullscreen(isFullscreen ? 0 : 1);
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);

        Debug.Log("Settings loaded.");
    }

    /// <summary>
    /// Resets all settings to default values and saves them.
    /// </summary>
    public void ResetToDefaults()
    {
        if (graphicsDropdown != null) graphicsDropdown.value = 2;
        if (fullscreenDropdown != null) fullscreenDropdown.value = 0;
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1.0f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 1.0f;

        SetGraphicsQuality(2);
        SetFullscreen(0);
        SetMasterVolume(1.0f);
        SetMusicVolume(1.0f);

        Debug.Log("Settings reset to default.");
    }
}
