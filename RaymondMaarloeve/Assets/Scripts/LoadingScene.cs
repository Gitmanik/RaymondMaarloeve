using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Dodaj na początku pliku

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Game";
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private TMP_Text loadingStageText; // Zmień typ na TMP_Text

    private float targetProgress = 0f;
    private float fillSpeed = 1.5f;

    private void Start()
    {
        if (progressBar != null)
            progressBar.value = 0f;
        StartCoroutine(LoadAsyncScene());
    }

    private void Update()
    {
        if (progressBar != null)
        {
            if (progressBar.value < targetProgress)
            {
                progressBar.value += fillSpeed * Time.deltaTime;
                if (progressBar.value > targetProgress)
                    progressBar.value = targetProgress;
            }
        }
    }

    private IEnumerator LoadAsyncScene()
    {
        UpdateLoadingStage("Starting to load Game scene");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        operation.allowSceneActivation = true;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f) * 0.66f;
            targetProgress = progress;
            UpdateLoadingStage($"Scene loading progress: {progress * 100:F0}%");
            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";
            yield return null;
        }

        UpdateLoadingStage("Looking for GameManager...");
        GameManager gameManager = null;
        while (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            yield return null;
        }
        UpdateLoadingStage("GameManager found!");

        UpdateLoadingStage("Waiting for LLM...");
        while (!gameManager.LlmServerReady)
        {
            targetProgress = 0.80f;
            yield return null;
        }
        UpdateLoadingStage("LLM ready!");

        UpdateLoadingStage("Waiting for MapGenerator...");
        while (MapGenerator.Instance == null || !MapGenerator.Instance.IsMapGenerated)
        {
            targetProgress = 0.90f;
            yield return null;
        }
        UpdateLoadingStage("Map generated!");

        targetProgress = 1f;
        UpdateLoadingStage("Finalizing...");
        if (progressText != null)
            progressText.text = "100%";
        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;

        UpdateLoadingStage("Unloading LoadingScene...");
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    private void UpdateLoadingStage(string message)
    {
        Debug.Log($"LoadingScene: {message}");
        if (loadingStageText != null)
        {
            loadingStageText.text = message;
        }
    }
}