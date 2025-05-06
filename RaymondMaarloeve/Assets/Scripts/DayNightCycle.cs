using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0, 24)] public float timeOfDay;
    public float dayDurationInMinutes = 1f; // 1 minuta = 24h

    [Header("Day Counter & UI")]
    private int currentDay = 1;

    [Header("Sun Light")]
    public Light directionalLight;
    public float maxSunIntensity = 1f;
    public float minSunIntensity = 0f;

    [Header("Skyboxes")]
    public Material daySky;    // materia� z shaderem Skybox/Cubemap lub Panoramic
    public Material nightSky;  // materia� na noc

    [Header("Exposure Settings")]
    public float maxExposure = 1.3f;  // jasno�� w ci�gu dnia
    public float minExposure = 0.3f;  // ciemno�� w nocy

    [Header("Skybox Rotation")]
    [Tooltip("Stopnie obrotu skyboxu na sekund�")]
    public float skyboxRotationSpeed = 1f;

    void Start()
    {
        // Ustaw pocz�tkowy skybox i UI
        RenderSettings.skybox = daySky;
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateDayText(currentDay);
    }

    void Update()
    {
        // 1) Aktualizacja czasu
        timeOfDay += Time.deltaTime / (dayDurationInMinutes * 60f) * 24f;
        if (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;
            currentDay++;
            if (DayBoxManager.Instance != null)
                DayBoxManager.Instance.UpdateDayText(currentDay);
        }

        // 2) Obr�t s�o�ca + intensywno��
        UpdateSun(timeOfDay);

        // 3) Skybox + ekspozycja
        UpdateSkybox(timeOfDay);

        // 4) UI godziny
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateHourText(timeOfDay);
    }

    void UpdateSun(float hour)
    {
        // Obr�t s�o�ca: od -90� (wsch�d) do +270� (zach�d)
        float sunAngle = (hour / 24f) * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Intensywno��: 0 noc�, max w po�udnie
        float normalized = Mathf.Clamp01(Mathf.Cos((hour / 24f - 0.25f) * 2f * Mathf.PI) * -1f);
        directionalLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalized);
    }

    void UpdateSkybox(float hour)
    {
        float t = hour / 24f;
        // Wyb�r skyboxu: dzie� do po�owy dnia, noc po po�owie
        Material target = (t < 0.5f) ? daySky : nightSky;
        RenderSettings.skybox = target;

        // Ekspozycja: ro�nie od �witu do po�udnia, potem maleje
        float exposureT = (t < 0.5f) ? (t * 2f) : (2f - t * 2f);
        float exp = Mathf.Lerp(minExposure, maxExposure, exposureT);
        target.SetFloat("_Exposure", exp);

        // Rotation (clouds)
        float rot = (Time.time * skyboxRotationSpeed) % 360f;
        target.SetFloat("_Rotation", rot);

        // Od�wie� globalne GI, by zmiany ekspozycji by�y widoczne
        DynamicGI.UpdateEnvironment();
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
}
