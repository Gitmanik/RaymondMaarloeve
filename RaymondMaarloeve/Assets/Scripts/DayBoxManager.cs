using UnityEngine;
using TMPro;

public class DayBoxManager : MonoBehaviour
{
    public static DayBoxManager Instance { get; private set; }

    [Header("Time Box")]
    [SerializeField] private GameObject timeBoxParent;

    [Header("Time Box Day Text Field")]
    [SerializeField] private TMP_Text dayTextField;

    [Header("Time Box Hour Text Field")]
    [SerializeField] private TMP_Text HourTextField;

    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // New method to update the hour text field
    public void UpdateHourText(float timeOfDay)
    {
        int hour = Mathf.FloorToInt(timeOfDay);
        // Calculate raw minutes, then round down to nearest quarter (15 minutes)
        int rawMinute = Mathf.FloorToInt((timeOfDay - hour) * 60f);
        int minute = (rawMinute / 15) * 15;
        // Format in "HH:MM" format
        HourTextField.text = hour.ToString("00") + ":" + minute.ToString("00");
    }
    
    // New method to update the day text field
    public void UpdateDayText(int currentDay)
    {
        dayTextField.text = "Day " + currentDay.ToString();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
