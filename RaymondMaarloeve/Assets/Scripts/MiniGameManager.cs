using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private Transform poolPanel;
    [SerializeField] private Transform sequencePanel;
    [SerializeField] private TMP_Dropdown suspectDropdown;
    [SerializeField] private GameObject historyBlockPrefab;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private string murderDescription;
    [SerializeField] private string[] blockTexts = new string[6];
    [SerializeField] private int triggerDay = 3;

    private List<string> correctBlocksOrder;
    private bool hasTriggered = false;
    private bool resultShown = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        miniGamePanel.SetActive(false);
        resultText.gameObject.SetActive(false);
        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(EndMiniGame);
    }

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

    public void StartMiniGame()
    {
        miniGamePanel.SetActive(true);
        resultText.gameObject.SetActive(false);
        resultShown = false;
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
        PopulateSuspects();
        GenerateBlocks();
    }

    private void PopulateSuspects()
    {
        suspectDropdown.ClearOptions();
        var names = GameManager.Instance.npcs
            .Select(n => n.gameObject.name)
            .ToList();
        suspectDropdown.AddOptions(names);
        suspectDropdown.RefreshShownValue();
    }

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

    public void EndMiniGame()
    {
        miniGamePanel.SetActive(false);
    }
}

public static class ListExtensions
{
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
