using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager for the “history fragments” mini‐game.  
/// Controls showing the mini‐game UI, populating suspects, generating draggable blocks,
/// evaluating the player’s selection, and displaying the result.
/// </summary>
public class MiniGameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the <see cref="MiniGameManager"/>.
    /// </summary>
    public static MiniGameManager Instance;

    /// <summary>
    /// The root panel GameObject for the mini‐game UI.
    /// </summary>
    [SerializeField] private GameObject miniGamePanel;

    /// <summary>
    /// Parent transform containing all draggable history blocks that have not yet been sorted.
    /// </summary>
    [SerializeField] private Transform poolPanel;

    /// <summary>
    /// Parent transform containing history blocks moved by the player as “real” fragments.
    /// </summary>
    [SerializeField] private Transform sequencePanel;

    /// <summary>
    /// Dropdown UI for selecting the suspect NPC by name.
    /// </summary>
    [SerializeField] private TMP_Dropdown suspectDropdown;

    /// <summary>
    /// Prefab for instantiating a history‐block UI element.
    /// </summary>
    [SerializeField] private GameObject historyBlockPrefab;

    /// <summary>
    /// Button to submit the player’s choices.
    /// </summary>
    [SerializeField] private Button submitButton;

    /// <summary>
    /// Button to cancel and close the mini‐game.
    /// </summary>
    [SerializeField] private Button cancelButton;

    /// <summary>
    /// Text element for showing success/failure messages.
    /// </summary>
    [SerializeField] private TextMeshProUGUI resultText;

    /// <summary>
    /// Array of correct (real) text fragments that must be moved by the player.
    /// </summary>
    private string[] realBlockTexts;

    /// <summary>
    /// Array of decoy (fake) text fragments that must remain in the pool.
    /// </summary>
    private string[] fakeBlockTexts;

    /// <summary>
    /// The in‐game day on which the mini‐game should automatically trigger.
    /// </summary>
    [SerializeField] private int triggerDay = 3;

    private bool hasTriggered = false;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Implements the singleton pattern by assigning <see cref="Instance"/>.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes UI state and button listeners.
    /// </summary>
    private void Start()
    {
        miniGamePanel.SetActive(false);
        resultText.gameObject.SetActive(false);
        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(EndMiniGame);
    }

    /// <summary>
    /// Called once per frame.  
    /// Checks the day counter and triggers the mini‐game when the target day is reached.
    /// </summary>
    private void Update()
    {
        if (hasTriggered) return;
        if (DayNightCycle.Instance == null) return;
        if (DayNightCycle.Instance.GetCurrentDay() < triggerDay) return;

        hasTriggered = true;
        StartMiniGame();
    }

    /// <summary>
    /// Sets up MiniGame blocks with given blocks  
    /// </summary>
    /// <param name="trueBlocks">List of true story blocks</param>
    /// <param name="fakeBlocks">List of false story blocks</param>
    public void Setup(List<string> trueBlocks, List<string> fakeBlocks)
    {
        realBlockTexts = trueBlocks.ToArray();
        fakeBlockTexts = fakeBlocks.ToArray();
    }

    /// <summary>
    /// Opens the mini‐game panel, hides the result text, populates suspects, and generates blocks.
    /// </summary>
    public void StartMiniGame()
    {
        miniGamePanel.SetActive(true);
        resultText.gameObject.SetActive(false);
        PopulateSuspects();
        GenerateBlocks();
    }

    /// <summary>
    /// Clears and repopulates the suspect dropdown with NPC names from the GameManager.
    /// </summary>
    private void PopulateSuspects()
    {
        suspectDropdown.ClearOptions();
        var names = GameManager.Instance.npcs
            .Select(npcGo => npcGo.GetComponent<NPC>().NpcName)
            .ToList();
        suspectDropdown.AddOptions(names);
        suspectDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Destroys any existing blocks in pool and target panels, then randomly instantiates new blocks.
    /// Real fragments are marked true; fake fragments marked false.
    /// </summary>
    private void GenerateBlocks()
    {
        // Clear existing children
        foreach (Transform c in poolPanel) Destroy(c.gameObject);
        foreach (Transform c in sequencePanel) Destroy(c.gameObject);

        // Combine real and fake texts into a single shuffled list
        var all = new List<(string text, bool isReal)>();
        foreach (var t in realBlockTexts) all.Add((t, true));
        foreach (var t in fakeBlockTexts) all.Add((t, false));
        all.Shuffle();

        // Instantiate blocks under the pool panel
        foreach (var pair in all)
        {
            var go = Instantiate(historyBlockPrefab, poolPanel);
            var block = go.AddComponent<HistoryBlock>();
            block.isReal = pair.isReal;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = pair.text;
        }
    }

    /// <summary>
    /// Called when the player clicks Submit.
    /// Validates that moved blocks match the real fragments, remaining match fakes, and suspect selection is correct.
    /// Displays a success or failure message accordingly.
    /// </summary>
    private void OnSubmit()
    {
        var moved = sequencePanel.GetComponentsInChildren<HistoryBlock>();
        var remaining = poolPanel.GetComponentsInChildren<HistoryBlock>();

        bool correctMoved = moved.Length == realBlockTexts.Length && moved.All(b => b.isReal);
        bool correctRemaining = remaining.Length == fakeBlockTexts.Length && remaining.All(b => !b.isReal);

        string chosen = suspectDropdown.options[suspectDropdown.value].text;
        string realName = GameManager.Instance.murdererNPC.GetComponent<NPC>().NpcName;
        bool suspectCorrect = chosen == realName;

        string resultMessage;
        if (correctMoved && correctRemaining && suspectCorrect)
            resultMessage = "Well done! You pointed out all real history fragments and identified the murderer.";
        else
            resultMessage = $"Wrong! The murderer was {realName}.";

        resultMessage += $"\n\n{GameManager.Instance.generatedHistory.story}";
        
        PlayerPrefs.SetString("GameResult", resultMessage);
        PlayerPrefs.Save();
        SceneManager.LoadScene("EndScene");
    }

    /// <summary>
    /// Closes the mini‐game panel without evaluating results.
    /// </summary>
    public void EndMiniGame()
    {
        miniGamePanel.SetActive(false);
    }
}

/// <summary>
/// Extension methods for generic lists.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Shuffles the elements of the list in place using Fisher–Yates algorithm.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
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
