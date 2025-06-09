using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Handles the loading process of a scene, including updating a progress bar and displaying loading messages.
/// </summary>
public class LoadingScene : MonoBehaviour
{
    /// <summary>
    /// The name of the scene to load.
    /// </summary>
    [SerializeField] private string sceneToLoad = "Game";

    /// <summary>
    /// The slider UI element used to display the loading progress.
    /// </summary>
    [SerializeField] private Slider progressBar;

    /// <summary>
    /// The text UI element used to display the percentage of loading progress.
    /// </summary>
    [SerializeField] private Text progressText;

    /// <summary>
    /// The TextMeshPro UI element used to display the current loading stage message.
    /// </summary>
    [SerializeField] private TMP_Text loadingStageText;

    /// <summary>
    /// The target progress value for the progress bar.
    /// </summary>
    private float targetProgress = 0f;

    /// <summary>
    /// The speed at which the progress bar fills.
    /// </summary>
    private float fillSpeed = 1.5f;

    /// <summary>
    /// Initializes the loading process and sets the progress bar to zero.
    /// </summary>
    private void Start()
    {
        if (progressBar != null)
            progressBar.value = 0f;
        StartCoroutine(LoadAsyncScene());
    }

    /// <summary>
    /// Updates the progress bar to gradually fill towards the target progress value.
    /// </summary>
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

    /// <summary>
    /// Handles the asynchronous loading of the specified scene, updating the progress bar and displaying loading messages.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
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

    /// <summary>
    /// Updates the loading stage message displayed in the UI.
    /// </summary>
    /// <param name="message">The message to display.</param>
    private void UpdateLoadingStage(string message)
    {
        Debug.Log($"LoadingScene: {message}");
        if (loadingStageText != null)
        {
            loadingStageText.text = message;
        }
    }
}