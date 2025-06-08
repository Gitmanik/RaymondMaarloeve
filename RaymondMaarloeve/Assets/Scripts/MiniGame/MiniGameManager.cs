/**
 * @file MiniGameManager.cs
 * @brief Manages the mini-game logic: showing the panel, generating history blocks, handling suspect selection, and evaluating the result.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/**
 * @class MiniGameManager
 * @brief Singleton responsible for controlling the flow of the mini-game.
 */
public class MiniGameManager : MonoBehaviour
{
    /** @brief Singleton instance. */
    public static MiniGameManager Instance;

    /** @brief The UI panel for the mini-game. */
    [SerializeField] private GameObject miniGamePanel;
    /** @brief Container for the pool of available blocks. */
    [SerializeField] private Transform poolPanel;
    /** @brief Container for the blocks arranged by the player. */
    [SerializeField] private Transform sequencePanel;
    /** @brief Dropdown listing suspects. */
    [SerializeField] private TMP_Dropdown suspectDropdown;
    /** @brief Prefab for a history block. */
    [SerializeField] private GameObject historyBlockPrefab;
    /** @brief Button to submit the solution. */
    [SerializeField] private Button submitButton;
    /** @brief Button to cancel the mini-game. */
    [SerializeField] private Button cancelButton;
    /** @brief Text field to display the result. */
    [SerializeField] private TextMeshProUGUI resultText;
    /** @brief Description of the crime shown on failure. */
    [SerializeField] private string murderDescription;
    /** @brief Array of block texts in the correct order. */
    [SerializeField] private string[] blockTexts = new string[6];
    /** @brief The day of the cycle on which the mini-game is triggered. */
    [SerializeField] private int triggerDay = 3;

    private List<string> correctBlocksOrder;
    private bool hasTriggered = false;
    private bool resultShown = false;

    /**
     * @brief Unity Awake — initializes the singleton.
     */
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /**
     * @brief Unity Start — sets up the UI and listeners.
     */
    void Start()
    {
        miniGamePanel.SetActive(false);
        resultText.gameObject.SetActive(false);
        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(EndMiniGame);
    }

    /**
     * @brief Unity Update — checks if the trigger day has been reached.
     */
    void Update()
    {
        if (hasTriggered) return;
        if (DayNightCycle.Instance == null) return;

        if (DayNightCycle.Instance.GetCurrentDay() >= triggerDay)
        {
            hasTriggered = true;
            StartMiniGame();
        }
    }

    /**
     * @brief Launches the mini-game panel and initializes content.
     */
    public void StartMiniGame()
    {
        miniGamePanel.SetActive(true);
        resultText.gameObject.SetActive(false);
        resultShown = false;
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
        PopulateSuspects();
        GenerateBlocks();
    }

    /**
     * @brief Populates the suspect dropdown with NPC names.
     */
    private void PopulateSuspects()
    {
        suspectDropdown.ClearOptions();
        var names = GameManager.Instance.npcs
            .Select(n => n.gameObject.name)
            .ToList();
        suspectDropdown.AddOptions(names);
        suspectDropdown.RefreshShownValue();
    }

    /**
     * @brief Generates history blocks: stores the correct order and shuffles them into the pool.
     */
    private void GenerateBlocks()
    {
        foreach (Transform c in poolPanel) Destroy(c.gameObject);
        foreach (Transform c in sequencePanel) Destroy(c.gameObject);

        correctBlocksOrder = blockTexts.ToList();
        var shuffled = blockTexts.ToList();
        shuffled.Shuffle();

        foreach (var txt in shuffled)
        {
            var go = Instantiate(historyBlockPrefab, poolPanel);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = txt;
        }
    }

    /**
     * @brief Handles the Submit/Close button click.
     * @details Checks the sequence and chosen suspect, then shows the result or closes the mini-game.
     */
    private void OnSubmit()
    {
        if (resultShown)
        {
            EndMiniGame();
            return;
        }

        var playerOrder = sequencePanel.Cast<Transform>()
            .Select(t => t.GetComponentInChildren<TextMeshProUGUI>().text)
            .ToList();
        bool sequenceCorrect = playerOrder.SequenceEqual(correctBlocksOrder);

        string chosen = suspectDropdown.options[suspectDropdown.value].text;
        bool suspectCorrect = chosen == GameManager.Instance.murdererNPC.gameObject.name;

        resultShown = true;
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        resultText.gameObject.SetActive(true);

        if (sequenceCorrect && suspectCorrect)
            resultText.text = "Congratulations Raymond! You solved the case.";
        else
            resultText.text = $"The murderer was {GameManager.Instance.murdererNPC.gameObject.name}. {murderDescription}";
    }

    /**
     * @brief Closes the mini-game panel.
     */
    public void EndMiniGame()
    {
        miniGamePanel.SetActive(false);
    }
}

/**
 * @class ListExtensions
 * @brief Extension methods for lists, including shuffle.
 */
public static class ListExtensions
{
    /**
     * @brief Shuffles the list using the Fisher–Yates algorithm.
     * @tparam T The element type.
     * @param list The list to shuffle.
     */
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
