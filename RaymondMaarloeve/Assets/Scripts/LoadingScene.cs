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
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            targetProgress = progress;
            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";

            if (operation.progress >= 0.9f)
            {
                targetProgress = 1f;
                if (progressText != null)
                    progressText.text = "100%";
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}