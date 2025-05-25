using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }


    [Range(0, 24)] public float timeOfDay;
    public float dayDurationInMinutes = 1f; // 1 minuta = 24h

    [Header("Day Counter & UI")]
    private int currentDay = 1;

    [Header("Sun Light")]
    public Light directionalLight;
    public float maxSunIntensity = 1f;
    public float minSunIntensity = 0f;

    [Header("Skyboxes")]
    public Material daySky;    // materia³ z shaderem Skybox/Cubemap lub Panoramic
    public Material nightSky;  // materia³ na noc

    [Header("Exposure Settings")]
    public float maxExposure = 1.3f;  // jasnoœæ w ci¹gu dnia
    public float minExposure = 0.3f;  // ciemnoœæ w nocy

    [Header("Skybox Rotation")]
    [Tooltip("Stopnie obrotu skyboxu na sekundê")]
    public float skyboxRotationSpeed = 1f;

    void Start()
    {
        // Ustaw pocz¹tkowy skybox i UI
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

        // 2) Obrót s³oñca + intensywnoœæ
        UpdateSun(timeOfDay);

        // 3) Skybox + ekspozycja
        UpdateSkybox(timeOfDay);

        // 4) UI godziny
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateHourText(timeOfDay);
    }

    void UpdateSun(float hour)
    {
        // Obrót s³oñca: od -90° (wschód) do +270° (zachód)
        float sunAngle = (hour / 24f) * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Intensywnoœæ: 0 noc¹, max w po³udnie
        float normalized = Mathf.Clamp01(Mathf.Cos((hour / 24f - 0.25f) * 2f * Mathf.PI) * -1f);
        directionalLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalized);
    }

    void UpdateSkybox(float hour)
    {
        float t = hour / 24f;
        // Wybór skyboxu: dzieñ do po³owy dnia, noc po po³owie
        Material target = (t < 0.5f) ? daySky : nightSky;
        RenderSettings.skybox = target;

        // Ekspozycja: roœnie od œwitu do po³udnia, potem maleje
        float exposureT = (t < 0.5f) ? (t * 2f) : (2f - t * 2f);
        float exp = Mathf.Lerp(minExposure, maxExposure, exposureT);
        target.SetFloat("_Exposure", exp);

        // Rotation (clouds)
        float rot = (Time.time * skyboxRotationSpeed) % 360f;
        target.SetFloat("_Rotation", rot);

        // Odœwie¿ globalne GI, by zmiany ekspozycji by³y widoczne
        DynamicGI.UpdateEnvironment();
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

}
