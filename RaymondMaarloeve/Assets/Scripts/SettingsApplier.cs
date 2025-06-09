using UnityEngine;

public class SettingsApplier : MonoBehaviour
{
    public AudioSource musicAudioSource; 

    void Start()
    {
        ApplySavedSettings();
    }

    void ApplySavedSettings()
    {
        int graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        QualitySettings.SetQualityLevel(graphicsQuality);

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;

        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        AudioListener.volume = masterVolume;

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume;
        }

        Debug.Log("[SettingsApplier] Settings applied:");
        Debug.Log("GraphicsQuality: " + graphicsQuality);
        Debug.Log("Fullscreen: " + isFullscreen);
        Debug.Log("MasterVolume: " + masterVolume);
        Debug.Log("MusicVolume: " + musicVolume);
    }
}
