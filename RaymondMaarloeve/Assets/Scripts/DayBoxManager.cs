using UnityEngine;
using TMPro;

/// <summary>
/// Manages the UI elements for displaying the current day and time in the game.
/// Provides methods to update the day and hour text fields dynamically.
/// </summary>
public class DayBoxManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the DayBoxManager class.
    /// </summary>
    public static DayBoxManager Instance { get; private set; }

    /// <summary>
    /// Parent GameObject for the time box UI.
    /// </summary>
    [Header("Time Box")]
    [SerializeField] private GameObject timeBoxParent;

    /// <summary>
    /// Text field for displaying the current day.
    /// </summary>
    [Header("Time Box Day Text Field")]
    [SerializeField] private TMP_Text dayTextField;

    /// <summary>
    /// Text field for displaying the current hour.
    /// </summary>
    [Header("Time Box Hour Text Field")]
    [SerializeField] private TMP_Text HourTextField;

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Updates the hour text field based on the given time of day.
    /// Formats the time in "HH:MM" format, rounding minutes to the nearest quarter hour.
    /// </summary>
    /// <param name="timeOfDay">The time of day as a float, where the integer part represents hours and the fractional part represents minutes.</param>
    public void UpdateHourText(float timeOfDay)
    {
        int hour = Mathf.FloorToInt(timeOfDay);
        // Calculate raw minutes, then round down to nearest quarter (15 minutes)
        int rawMinute = Mathf.FloorToInt((timeOfDay - hour) * 60f);
        int minute = (rawMinute / 15) * 15;
        // Format in "HH:MM" format
        HourTextField.text = hour.ToString("00") + ":" + minute.ToString("00");
    }

    /// <summary>
    /// Updates the day text field with the given day number.
    /// </summary>
    /// <param name="currentDay">The current day number to display.</param>
    public void UpdateDayText(int currentDay)
    {
        dayTextField.text = "Day " + currentDay.ToString();
    }

    /// <summary>
    /// Unity Start method. Currently unused.
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Unity Update method. Currently unused.
    /// </summary>
    void Update()
    {

    }
}
