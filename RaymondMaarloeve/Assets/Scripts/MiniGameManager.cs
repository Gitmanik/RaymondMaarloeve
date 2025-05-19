using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private Transform poolPanel;
    [SerializeField] private Transform sequencePanel;

    [Header("Prefabs & Buttons")]
    [SerializeField] private GameObject historyBlockPrefab;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    [Header("Historia")]
    [TextArea] public string fullHistoryLog;

    [Header("Trigger Settings")]
    [Tooltip("In which day minigame starts")]
    [SerializeField] private int triggerDay = 1;

    private List<string> correctBlocksOrder;
    private bool hasTriggered = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        miniGamePanel.SetActive(false);
        submitButton.onClick.AddListener(OnSubmit);
    }

    void Update()
    {
        if (DayNightCycle.Instance == null)
        {
            Debug.LogWarning("[MiniGameManager] No DayNightCycle.Instance!");
            return;
        }

        int currentDay = DayNightCycle.Instance.GetCurrentDay();
        Debug.Log($"[MiniGameManager] Update – currentDay={currentDay}, hasTriggered={hasTriggered}");

        if (!hasTriggered && currentDay >= triggerDay)
        {
            hasTriggered = true;
            Debug.Log($"[MiniGameManager] Day {currentDay}  -> starting StartMiniGame()");
            StartMiniGame();
        }
    }

    public void StartMiniGame()
    {
        Debug.Log("[MiniGameManager] StartMiniGame() ");
        miniGamePanel.SetActive(true);
        GenerateBlocks();
    }


    private void GenerateBlocks()
    {
        foreach (Transform c in poolPanel) Destroy(c.gameObject);
        foreach (Transform c in sequencePanel) Destroy(c.gameObject);

        var real = fullHistoryLog
            .Split(new[] { '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim() + ".")
            .ToList();

        correctBlocksOrder = new List<string>(real);

        var fake = new List<string> {
            "Fake history: ajsoifdjaoisf",
            "Fake history: sakjfnaliksjfpoiajsnfoina"
        };
        real.AddRange(fake);
        real.Shuffle();

        foreach (var txt in real)
        {
            var go = Instantiate(historyBlockPrefab, poolPanel);
            var uiText = go.GetComponentInChildren<Text>();
            if (uiText != null) uiText.text = txt;
        }
    }

    private void OnSubmit()
    {
        var playerOrder = sequencePanel.Cast<Transform>()
            .Select(t => t.GetComponentInChildren<Text>().text)
            .ToList();

        bool sequenceCorrect = playerOrder.SequenceEqual(correctBlocksOrder);

        if (sequenceCorrect)
            Debug.Log("You solved the case!");
        else
            Debug.Log("You didn't solve the case");

        EndMiniGame();
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
