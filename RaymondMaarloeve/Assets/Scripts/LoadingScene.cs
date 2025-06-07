using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Game";
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;

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
        Debug.Log("LoadingScene: Starting to load Game scene");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        operation.allowSceneActivation = true;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f) * 0.66f;
            targetProgress = progress;
            Debug.Log($"LoadingScene: Scene loading progress: {progress:F2}");
            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";
            yield return null;
        }

        Debug.Log("LoadingScene: Looking for GameManager...");
        GameManager gameManager = null;
        while (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            yield return null;
        }
        Debug.Log("LoadingScene: GameManager found!");

        Debug.Log("LoadingScene: Waiting for LLM...");
        while (!gameManager.LlmServerReady)
        {
            targetProgress = 0.80f;
            yield return null;
        }
        Debug.Log("LoadingScene: LLM ready!");

        Debug.Log("LoadingScene: Waiting for MapGenerator...");
        while (MapGenerator.Instance == null || !MapGenerator.Instance.IsMapGenerated)
        {
            targetProgress = 0.90f;
            yield return null;
        }
        Debug.Log("LoadingScene: Map generated!");

        // Ostatni etap ładowania
        targetProgress = 1f;
        if (progressText != null)
            progressText.text = "100%";
        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;

        // (Opcjonalnie) Unload scenę ładowania
        SceneManager.UnloadSceneAsync("LoadingScene");
    }
}