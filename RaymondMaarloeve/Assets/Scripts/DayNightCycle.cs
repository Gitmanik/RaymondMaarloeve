using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0, 24)] public float timeOfDay;
    public float dayDurationInMinutes = 1f; // np. 1 minuta = 24h
    public Light directionalLight;

    void Update()
    {
        timeOfDay += Time.deltaTime / (dayDurationInMinutes * 60f) * 24f;
        if (timeOfDay >= 24f) timeOfDay = 0f;

        UpdateLighting(timeOfDay);
    }

    void UpdateLighting(float hour)
    {
        float angle = (hour / 24f) * 360f;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(angle - 90f, 170f, 0));
    }
}
