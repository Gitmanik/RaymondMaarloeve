using UnityEngine;

/// <summary>
/// Manages the day-night cycle including sun movement, skybox transition,
/// light intensity, and visual exposure throughout a 24-hour simulated day.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    /// <summary>Current time of day (0–24).</summary>
    [Header("Time Settings")]
    [Range(0, 24)] public float timeOfDay;

    /// <summary>Duration of one full in-game day in real-world minutes.</summary>
    public float dayDurationInMinutes = 1f;

    /// <summary>Current simulated day counter.</summary>
    [Header("Day Counter & UI")]
    private int currentDay = 1;

    /// <summary>Directional light acting as the sun.</summary>
    [Header("Sun Light")]
    public Light directionalLight;

    /// <summary>Maximum sun intensity at noon.</summary>
    public float maxSunIntensity = 1f;

    /// <summary>Minimum sun intensity at night.</summary>
    public float minSunIntensity = 0f;

    /// <summary>Skybox material for daytime.</summary>
    [Header("Skyboxes")]
    public Material daySky;

    /// <summary>Skybox material for nighttime.</summary>
    public Material nightSky;

    /// <summary>Maximum skybox exposure (brightness) during day.</summary>
    [Header("Exposure Settings")]
    public float maxExposure = 1.3f;

    /// <summary>Minimum skybox exposure (darkness) during night.</summary>
    public float minExposure = 0.3f;

    /// <summary>Speed of skybox rotation in degrees per second.</summary>
    [Header("Skybox Rotation")]
    [Tooltip("Stopnie obrotu skyboxu na sekundê")]
    public float skyboxRotationSpeed = 1f;

    /// <summary>
    /// Initializes the scene with the correct skybox and updates the day UI.
    /// </summary>
    void Start()
    {
        RenderSettings.skybox = daySky;
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateDayText(currentDay);
    }

    /// <summary>
    /// Updates time, lighting, skybox, and UI every frame.
    /// </summary>
    void Update()
    {
        timeOfDay += Time.deltaTime / (dayDurationInMinutes * 60f) * 24f;
        if (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;
            currentDay++;
            if (DayBoxManager.Instance != null)
                DayBoxManager.Instance.UpdateDayText(currentDay);
        }

        UpdateSun(timeOfDay);
        UpdateSkybox(timeOfDay);

        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateHourText(timeOfDay);
    }

    /// <summary>
    /// Rotates the directional light and adjusts its intensity based on time of day.
    /// </summary>
    /// <param name="hour">Current time of day in hours.</param>
    void UpdateSun(float hour)
    {
        float sunAngle = (hour / 24f) * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float normalized = Mathf.Clamp01(Mathf.Cos((hour / 24f - 0.25f) * 2f * Mathf.PI) * -1f);
        directionalLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalized);
    }

    /// <summary>
    /// Adjusts the active skybox material, its exposure, and rotation according to time of day.
    /// </summary>
    /// <param name="hour">Current time of day in hours.</param>
    void UpdateSkybox(float hour)
    {
        float t = hour / 24f;
        Material target = (t < 0.5f) ? daySky : nightSky;
        RenderSettings.skybox = target;

        float exposureT = (t < 0.5f) ? (t * 2f) : (2f - t * 2f);
        float exp = Mathf.Lerp(minExposure, maxExposure, exposureT);
        target.SetFloat("_Exposure", exp);

        float rot = (Time.time * skyboxRotationSpeed) % 360f;
        target.SetFloat("_Rotation", rot);

        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// Returns the current in-game day.
    /// </summary>
    public int GetCurrentDay()
    {
        return currentDay;
    }
}
