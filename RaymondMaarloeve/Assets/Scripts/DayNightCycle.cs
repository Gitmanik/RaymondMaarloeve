using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0, 24)] public float timeOfDay;
    public float dayDurationInMinutes = 1f; // 1 minute = 24h
    public Light directionalLight;

    private int currentDay = 1; // Current day counter

    void Start()
    {
        // Initialize the day text in DayBoxManager
        if (DayBoxManager.Instance != null)
        {
            DayBoxManager.Instance.UpdateDayText(currentDay);
        }
    }

    void Update()
    {
        timeOfDay += Time.deltaTime / (dayDurationInMinutes * 60f) * 24f;
        if (timeOfDay >= 24f)
        {
            timeOfDay = 0f;
            currentDay++; // Advance to the next day

            // Optionally update day text in UI
            if (DayBoxManager.Instance != null)
            {
                DayBoxManager.Instance.UpdateDayText(currentDay);
            }
        }

        UpdateLighting(timeOfDay);

        // Update the hour text in DayBoxManager
        if (DayBoxManager.Instance != null)
        {
            DayBoxManager.Instance.UpdateHourText(timeOfDay);
        }
    }

    void UpdateLighting(float hour)
    {
        float angle = (hour / 24f) * 360f;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(angle - 90f, 170f, 0));
    }

    // Skeleton function to return the current day number
    public int GetCurrentDay()
    {
        return currentDay;
    }
}
