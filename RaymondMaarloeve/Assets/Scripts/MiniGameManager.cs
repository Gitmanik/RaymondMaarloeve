using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private Transform poolPanel;
    [SerializeField] private Transform sequencePanel;

    [Header("Prefab")]
    [SerializeField] private GameObject historyBlockPrefab;

    [Header("Buttons")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    [Header("Six Blocks (in correct order)")]
    [SerializeField] private string[] blockTexts = new string[6];

    [Header("Trigger Settings")]
    [SerializeField] private int triggerDay = 3;

    private List<string> correctBlocksOrder;
    private bool hasTriggered = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (miniGamePanel == null) Debug.LogError("[MiniGameManager] miniGamePanel is not assigned!");
        if (poolPanel == null) Debug.LogError("[MiniGameManager] poolPanel is not assigned!");
        if (sequencePanel == null) Debug.LogError("[MiniGameManager] sequencePanel is not assigned!");
        if (historyBlockPrefab == null) Debug.LogError("[MiniGameManager] historyBlockPrefab is not assigned!");
        if (submitButton == null) Debug.LogError("[MiniGameManager] submitButton is not assigned!");
        if (cancelButton == null) Debug.LogError("[MiniGameManager] cancelButton is not assigned!");
        if (blockTexts == null || blockTexts.Length != 6) Debug.LogError("[MiniGameManager] blockTexts must have length 6!");

        miniGamePanel.SetActive(false);
        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(OnCancel);
    }

    void Update()
    {
        if (hasTriggered) return;
        if (DayNightCycle.Instance == null) return;

        int currentDay = DayNightCycle.Instance.GetCurrentDay();
        ///Debug.Log($"[MiniGameManager] Update – day={currentDay}, hasTriggered={hasTriggered}");

        if (currentDay >= triggerDay)
        {
            hasTriggered = true;
            Debug.Log($"[MiniGameManager] Day {currentDay} >= {triggerDay} → StartMiniGame()");
            StartMiniGame();
        }
    }

    public void StartMiniGame()
    {
        Debug.Log("[MiniGameManager] ▶ StartMiniGame()");
        miniGamePanel.SetActive(true);
        GenerateBlocks();
    }

    private void GenerateBlocks()
    {
        Debug.Log($"[MiniGameManager] ▶ GenerateBlocks() – blockTexts.Length = {blockTexts.Length}");
        for (int i = 0; i < blockTexts.Length; i++)
            Debug.Log($"   blockTexts[{i}] = '{blockTexts[i]}'");

        foreach (Transform c in poolPanel) Destroy(c.gameObject);
        foreach (Transform c in sequencePanel) Destroy(c.gameObject);

        correctBlocksOrder = new List<string>(blockTexts);

        List<string> shuffled = new List<string>(blockTexts);
        shuffled.Shuffle();

        Debug.Log($"[MiniGameManager] ▶ GenerateBlocks() – after Shuffle, Count = {shuffled.Count}");
        foreach (var txt in shuffled)
            Debug.Log($"   shuffled element = '{txt}'");

        if (historyBlockPrefab.GetComponentInChildren<TextMeshProUGUI>() == null)
            Debug.LogError("[MiniGameManager] Prefab is missing TextMeshProUGUI in a child!");

        foreach (string txt in shuffled)
        {
            GameObject go = Instantiate(historyBlockPrefab, poolPanel);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = txt;
            else Debug.LogError("[MiniGameManager] Instantiated prefab is missing TextMeshProUGUI in a child!");
        }

        Debug.Log($"[MiniGameManager] ▶ poolPanel.childCount = {poolPanel.childCount}");
    }

    private void OnSubmit()
    {
        List<string> playerOrder = sequencePanel.Cast<Transform>()
            .Select(t => t.GetComponentInChildren<TextMeshProUGUI>().text)
            .ToList();

        Debug.Log("[MiniGameManager]  OnSubmit() – player arranged sequence:");
        playerOrder.ForEach(s => Debug.Log($"   '{s}'"));

        bool sequenceCorrect = playerOrder.SequenceEqual(correctBlocksOrder);

        if (sequenceCorrect)
            Debug.Log("[MiniGameManager]  Correct sequence!");
        else
            Debug.Log("[MiniGameManager]  Incorrect sequence!");

        EndMiniGame();
    }

    private void OnCancel()
    {
        Debug.Log("[MiniGameManager]  OnCancel() – minigame canceled");
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
