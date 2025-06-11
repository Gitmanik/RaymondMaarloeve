using System;
using UnityEngine;
using UnityEngine.UI;

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

    public bool enableTimePass = false;

    [Header("Night Time Settings")]
    public float nightTimeSpeed = 20f;
    public float nightOverlayMaxAlpha = 1f;
    public float requiredSleepingNPCPercentage = 0.80f;
    
    private bool isNightActive = false;
    
    private float normalTimeSpeed = 1f;
    private UnityEngine.UI.Image nightOverlay; // referencja do ciemnej nakładki
    
    void Start()
    {
        // Ustaw pocz¹tkowy skybox i UI
        RenderSettings.skybox = daySky;
        if (DayBoxManager.Instance != null)
            DayBoxManager.Instance.UpdateDayText(currentDay);
        
        // Dodaj ciemną nakładkę
        CreateNightOverlay();
    }
    
    private void CreateNightOverlay()
    {
        // Stwórz nowy obiekt Canvas (jeśli nie istnieje)
        GameObject overlayCanvas = new GameObject("NightOverlay Canvas");
        Canvas canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0; // nad grą, pod UI
        
        // Dodaj CanvasScaler
        CanvasScaler scaler = overlayCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        // Stwórz obiekt z Image
        GameObject overlayObj = new GameObject("NightOverlay");
        overlayObj.transform.SetParent(overlayCanvas.transform, false);
        nightOverlay = overlayObj.AddComponent<UnityEngine.UI.Image>();
        nightOverlay.color = new Color(0, 0, 0, 0); // czarny, początkowo przezroczysty
        
        // Ustaw rozmiar na cały ekran
        RectTransform rect = overlayObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }
    
    void Update()
    {
        if (!enableTimePass)
            return;
        
        // Sprawdź warunki rozpoczęcia nocy
        if (!isNightActive)
        {
            if (timeOfDay >= 22f) // Najpierw sprawdź czy jest 22:00
            {
                isNightActive = true;
            }
            else if (timeOfDay >= 21.25f) // Jeśli jest między 21:15 a 22:00
            {
                if (checkNPCsSleep(requiredSleepingNPCPercentage)) // Dopiero wtedy sprawdź NPCs
                {
                    isNightActive = true;
                }
            }
        }
        
        // Sprawdź warunki zakończenia nocy
        if (isNightActive && timeOfDay >= 7f && timeOfDay < 8f)
        {
            isNightActive = false;
        }
        
        // Ustaw prędkość upływu czasu
        float currentTimeSpeed = isNightActive ? nightTimeSpeed : normalTimeSpeed;
        
        // Aktualizacja czasu z uwzględnieniem prędkości
        timeOfDay += (Time.deltaTime / (dayDurationInMinutes * 60f) * 24f) * currentTimeSpeed;
        
        // Zarządzaj przezroczystością nakładki nocnej
        if (nightOverlay != null)
        {
            float targetAlpha = isNightActive ? nightOverlayMaxAlpha : 0f;
            Color currentColor = nightOverlay.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * 2f);
            nightOverlay.color = currentColor;
        }
        
        // 1) Aktualizacja czasu
        //timeOfDay += Time.deltaTime / (dayDurationInMinutes * 60f) * 24f;
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

    public string GetCurrentTimeText()
    {
        int hour = Mathf.FloorToInt(timeOfDay);
        int minute = Mathf.FloorToInt((timeOfDay - hour) * 60f);
        // Format in "HH:MM" format
        return hour.ToString("00") + ":" + minute.ToString("00");
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

}