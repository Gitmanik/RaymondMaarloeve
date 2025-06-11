using System;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Day-Night Cycle Settings")]
    /// <summary>
    /// Current time of day, ranging from 0 to 24.
    /// </summary>
    [Range(0, 24)] public float timeOfDay;

    /// <summary>
    /// Duration of a full day in minutes.
    /// 1 minute equals 24 hours in-game.
    /// </summary>
    public float dayDurationInMinutes = 1f;

    /// <summary>
    /// Current day counter.
    /// </summary>
    private int currentDay = 1;

    [Header("Sun Light")]
    /// <summary>
    /// Reference to the directional light representing the sun.
    /// </summary>
    public Light directionalLight;

    /// <summary>
    /// Maximum intensity of the sun during the day.
    /// </summary>
    public float maxSunIntensity = 1f;

    /// <summary>
    /// Minimum intensity of the sun during the night.
    /// </summary>
    public float minSunIntensity = 0f;

    [Header("Skybox Settings")]
    /// <summary>
    /// Skybox material used during the day.
    /// Material with Skybox/Cubemap or Panoramic shader.
    /// </summary>
    public Material daySky;

    /// <summary>
    /// Skybox material used during the night.
    /// </summary>
    public Material nightSky;

    [Header("Exposure Settings")]
    /// <summary>
    /// Maximum exposure level during the day.
    /// </summary>
    public float maxExposure = 1.3f;

    /// <summary>
    /// Minimum exposure level during the night.
    /// </summary>
    public float minExposure = 0.3f;

    /// <summary>
    /// Rotation speed of the skybox in degrees per second.
    /// </summary>
    [Tooltip("Degrees of skybox rotation per second")]
    public float skyboxRotationSpeed = 1f;

    /// <summary>
    /// Enables or disables the passage of time.
    /// </summary>
    public bool enableTimePass = false;

    [Header("Night Settings")]
    /// <summary>
    /// Speed multiplier for time progression during the night.
    /// </summary>
    public float nightTimeSpeed = 20f;

    /// <summary>
    /// Maximum alpha value for the night overlay.
    /// </summary>
    public float nightOverlayMaxAlpha = 1f;

    /// <summary>
    /// Required percentage of NPCs sleeping to trigger night conditions.
    /// </summary>
    public float requiredSleepingNPCPercentage = 0.80f;

    /// <summary>
    /// Indicates whether night conditions are active.
    /// </summary>
    private bool isNightActive = false;

    /// <summary>
    /// Normal speed multiplier for time progression.
    /// </summary>
    private float normalTimeSpeed = 1f;

    /// <summary>
    /// Reference to the night overlay UI element.
    /// </summary>
    private UnityEngine.UI.Image nightOverlay;

    /// <summary>
    /// Initializes the day-night cycle and UI elements.
    /// </summary>
    void Start()
    {
        // Set initial skybox and UI
        RenderSettings.skybox = daySky;
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateDayText(currentDay);

        // Add night overlay
        CreateNightOverlay();
    }

    /// <summary>
    /// Creates the night overlay UI element.
    /// </summary>
    private void CreateNightOverlay()
    {
        // Create a new Canvas object (if it doesn't exist)
        GameObject overlayCanvas = new GameObject("NightOverlay Canvas");
        Canvas canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0; // above the game, below UI

        // Add CanvasScaler
        CanvasScaler scaler = overlayCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create an object with Image
        GameObject overlayObj = new GameObject("NightOverlay");
        overlayObj.transform.SetParent(overlayCanvas.transform, false);
        nightOverlay = overlayObj.AddComponent<UnityEngine.UI.Image>();
        nightOverlay.color = new Color(0, 0, 0, 0); // black, initially transparent

        // Set size to full screen
        RectTransform rect = overlayObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Updates the day-night cycle based on time progression.
    /// </summary>
    void Update()
    {
        if (!enableTimePass)
            return;

        // Check conditions to start the night
        if (!isNightActive)
        {
            if (timeOfDay >= 22f) // First check if it's 22:00
            {
                isNightActive = true;
            }
            else if (timeOfDay >= 21.25f) // If it's between 21:15 and 22:00
            {
                if (checkNPCsSleep(requiredSleepingNPCPercentage)) // Then check NPCs
                {
                    isNightActive = true;
                }
            }
        }

        // Check conditions to end the night
        if (isNightActive && timeOfDay >= 7f && timeOfDay < 8f)
        {
            isNightActive = false;
        }

        // Set time progression speed
        float currentTimeSpeed = isNightActive ? nightTimeSpeed : normalTimeSpeed;

        // Update time considering speed
        timeOfDay += (Time.deltaTime / (dayDurationInMinutes * 60f) * 24f) * currentTimeSpeed;

        // Manage night overlay transparency
        if (nightOverlay != null)
        {
            float targetAlpha = isNightActive ? nightOverlayMaxAlpha : 0f;
            Color currentColor = nightOverlay.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * 2f);
            nightOverlay.color = currentColor;
        }

        // Update time
        if (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;
            currentDay++;
            if (DayBoxManager.Instance != null)
                DayBoxManager.Instance.UpdateDayText(currentDay);
        }

        // Update sun rotation and intensity
        UpdateSun(timeOfDay);

        // Update skybox and exposure
        UpdateSkybox(timeOfDay);

        // Update UI hour
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateHourText(timeOfDay);
    }

    /// <summary>
    /// Checks if the required percentage of NPCs are sleeping.
    /// </summary>
    /// <param name="requiredSleepingNPCPercentage">Percentage of NPCs required to be sleeping.</param>
    /// <returns>True if the required percentage of NPCs are sleeping, false otherwise.</returns>
    bool checkNPCsSleep(float requiredSleepingNPCPercentage = 0.5f)
    {
        NPC[] allNPCs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        if (allNPCs.Length > 0)
        {
            int sleepingNPCs = 0;
            foreach (NPC npc in allNPCs)
            {
                if (npc.GetCurrentDecision() is GoToSleepDecision)
                {
                    if (((VisitBuildingDecision)npc.GetCurrentDecision()).reachedBuilding)
                        sleepingNPCs++;
                }
            }

            float sleepingPercentage = (float)sleepingNPCs / allNPCs.Length;
            return sleepingPercentage >= requiredSleepingNPCPercentage;
        }

        return true;
    }

    /// <summary>
    /// Updates the sun's rotation and intensity based on the current time.
    /// </summary>
    /// <param name="hour">Current hour of the day.</param>
    void UpdateSun(float hour)
    {
        // Sun rotation: from -90° (sunrise) to +270° (sunset)
        float sunAngle = (hour / 24f) * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Intensity: 0 at night, max at noon
        float normalized = Mathf.Clamp01(Mathf.Cos((hour / 24f - 0.25f) * 2f * Mathf.PI) * -1f);
        directionalLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalized);
    }

    /// <summary>
    /// Updates the skybox and exposure settings based on the current time.
    /// </summary>
    /// <param name="hour">Current hour of the day.</param>
    void UpdateSkybox(float hour)
    {
        float t = hour / 24f;
        // Skybox selection: day until halfway through, night after halfway
        Material target = (t < 0.5f) ? daySky : nightSky;
        RenderSettings.skybox = target;

        // Exposure: increases from dawn to noon, then decreases
        float exposureT = (t < 0.5f) ? (t * 2f) : (2f - t * 2f);
        float exp = Mathf.Lerp(minExposure, maxExposure, exposureT);
        target.SetFloat("_Exposure", exp);

        // Rotation (clouds)
        float rot = (Time.time * skyboxRotationSpeed) % 360f;
        target.SetFloat("_Rotation", rot);

        // Refresh global GI to make exposure changes visible
        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// Gets the current day number.
    /// </summary>
    /// <returns>Current day number.</returns>
    public int GetCurrentDay()
    {
        return currentDay;
    }

    /// <summary>
    /// Gets the current time in "HH:MM" format.
    /// </summary>
    /// <returns>Current time as a string.</returns>
    public string GetCurrentTimeText()
    {
        int hour = Mathf.FloorToInt(timeOfDay);
        int minute = Mathf.FloorToInt((timeOfDay - hour) * 60f);
        return hour.ToString("00") + ":" + minute.ToString("00");
    }

    /// <summary>
    /// Ensures a single instance of the DayNightCycle class.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}