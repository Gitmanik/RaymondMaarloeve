using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages communication with the LLM server, including model registration, loading, unloading, and chat requests.
/// Handles request queuing and ensures only one POST request is processed at a time.
/// </summary>
public class LlmManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the LlmManager.
    /// </summary>
    public static LlmManager Instance;
    /// <summary>
    /// Base URL of the LLM server API.
    /// </summary>
    private string BaseUrl;
    /// <summary>
    /// Indicates whether the manager is connected to the LLM server.
    /// </summary>
    public bool IsConnected { get; private set; }
    /// <summary>
    /// Queue of POST requests to be processed sequentially.
    /// </summary>
    private Queue<IEnumerator> postRequestQueue = new Queue<IEnumerator>();
    /// <summary>
    /// Indicates if the POST request queue is currently being processed.
    /// </summary>
    private bool isProcessingQueue = false;

    /// <summary>
    /// Sets up the LLM manager with the specified API base URL.
    /// </summary>
    /// <param name="api">Base URL of the LLM server API.</param>
    public void Setup(string api)
    {
        BaseUrl = api;
    }
    
    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Sends a GET request to the specified endpoint and deserializes the response to type T.
    /// </summary>
    /// <typeparam name="T">Type to deserialize the response to.</typeparam>
    /// <param name="endpoint">API endpoint.</param>
    /// <param name="onSuccess">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    /// <returns>Coroutine enumerator.</returns>
    public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError) where T : class
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/{endpoint}"))
        {
            Debug.Log($"LlmManager: Get request: /{endpoint}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Web request failed: {request.error}");
                yield break;
            }

            var content = request.downloadHandler.text;
            Debug.Log($"LlmManager: Get response: {content}");
            var result = JsonUtility.FromJson<T>(content);
            onSuccess?.Invoke(result);
        }
    }

    /// <summary>
    /// Queues a POST request, ensuring that only one executes at a time.
    /// </summary>
    /// <typeparam name="T">Type to deserialize the response to.</typeparam>
    /// <typeparam name="TRequest">Type of the request data.</typeparam>
    /// <param name="endpoint">API endpoint.</param>
    /// <param name="data">Request data.</param>
    /// <param name="onSuccess">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void QueuePostRequest<T, TRequest>(string endpoint, TRequest data, Action<T> onSuccess, Action<string> onError) 
        where T : class 
        where TRequest : class
    {
        // Add the request to the queue
        postRequestQueue.Enqueue(Post<T, TRequest>(endpoint, data, onSuccess, onError));
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessPostQueue());
        }
    }

    /// <summary>
    /// Processes the POST request queue sequentially.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator ProcessPostQueue()
    {
        isProcessingQueue = true;

        while (postRequestQueue.Count > 0)
        {
            var request = postRequestQueue.Dequeue();
            yield return StartCoroutine(request);
        }

        isProcessingQueue = false;
    }

    /// <summary>
    /// Sends a POST request with data and deserializes the response to type T.
    /// </summary>
    /// <typeparam name="T">Type to deserialize the response to.</typeparam>
    /// <typeparam name="TRequest">Type of the request data.</typeparam>
    /// <param name="endpoint">API endpoint.</param>
    /// <param name="data">Request data.</param>
    /// <param name="onSuccess">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator Post<T, TRequest>(string endpoint, TRequest data, Action<T> onSuccess, Action<string> onError) 
        where T : class 
        where TRequest : class
    {
        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/{endpoint}", "POST"))
        {
            var json = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"LlmManager: Post request: {json}");
            
            yield return request.SendWebRequest();

            var responseContent = request.downloadHandler.text;
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"LlmManager: Post request failed ({request.error}): {responseContent}");
                yield break;
            }
            Debug.Log($"LlmManager: Post response: {responseContent}");

            var result = JsonUtility.FromJson<T>(responseContent);
            onSuccess?.Invoke(result);
        }
    }

    #region Endpoint handlers
    /// <summary>
    /// Gets the status of the LLM server.
    /// </summary>
    /// <param name="onComplete">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void Status(Action<StatusDTO> onComplete, Action<string> onError)
    {
        StartCoroutine(Get<StatusDTO>("status", onComplete, onError));
    }

    /// <summary>
    /// Loads a model on the LLM server.
    /// </summary>
    /// <param name="modelID">Unique identifier for the model.</param>
    /// <param name="path">File system path to the model file.</param>
    /// <param name="onComplete">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void LoadModel(string modelID, string path, Action<MessageDTO> onComplete, Action<string> onError)
    {
        var data = new LoadModelDTO()
        {
            model_id = modelID,
            model_path = path,
            f16_kv = true,
            n_ctx = 1024,
            n_parts = -1,
            seed = 42, // TODO: Make it randomized
            n_gpu_layers = -1,
        };
        
        QueuePostRequest<MessageDTO, LoadModelDTO>("load", data, onComplete, onError);
    }

    /// <summary>
    /// Unloads a model from the LLM server.
    /// </summary>
    /// <param name="modelID">Unique identifier for the model.</param>
    /// <param name="onComplete">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void UnloadModel(string modelID, Action<MessageDTO> onComplete, Action<string> onError)
    {
        var data = new UnloadModelRequestDTO()
        {
            model_id = modelID
        };
        
        QueuePostRequest<MessageDTO, UnloadModelRequestDTO>("unload", data, onComplete, onError);
    }

    /// <summary>
    /// Registers a model with the LLM server, making it available for loading and inference.
    /// </summary>
    /// <param name="modelID">Unique identifier for the model.</param>
    /// <param name="path">File system path to the model file.</param>
    /// <param name="onComplete">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void Register(string modelID, string path, Action<MessageDTO> onComplete, Action<string> onError)
    {
        var data = new RegisterDTO()
        {
            model_id = modelID,
            model_path = path,
        };
        
        QueuePostRequest<MessageDTO, RegisterDTO>("register", data, onComplete, onError);
    }
    
    /// <summary>
    /// Sends a chat request to the LLM server using the specified model and message history.
    /// </summary>
    /// <param name="modelID">Unique identifier for the model to use.</param>
    /// <param name="messages">List of messages forming the conversation history.</param>
    /// <param name="onComplete">Callback on successful response.</param>
    /// <param name="onError">Callback on error.</param>
    public void Chat(string modelID, List<Message> messages, Action<ChatResponseDTO> onComplete, Action<string> onError)
    {
        var data = new ChatRequestDTO()
        {
            model_id = modelID,
            messages = messages,
            max_tokens = 1500,
            temperature = 0.5f,
            top_p = 0.95f,
        };
        QueuePostRequest<ChatResponseDTO, ChatRequestDTO>("chat", data, onComplete, onError);
    }
    #endregion
    
    /// <summary>
    /// Connects to the LLM server and unloads all currently loaded models.
    /// </summary>
    /// <param name="onComplete">Callback with connection status (true if healthy).</param>
    public void Connect(Action<bool> onComplete)
    {
        Status(
            healthData => 
            {
                IsConnected = healthData.healthy;
                
                foreach (var model in healthData.models)
                    UnloadModel(model, GenericComplete, Debug.LogError);
                
                onComplete?.Invoke(IsConnected);
            },
            error => 
            {
                Debug.LogError("Could not connect to LLM Server: " + error);
                IsConnected = false;
                onComplete?.Invoke(IsConnected);
            });
    }

    /// <summary>
    /// Generic callback for handling MessageDTO responses, logs success or error messages.
    /// </summary>
    /// <param name="message">The message DTO returned from the server.</param>
    public void GenericComplete(MessageDTO message)
    {
        if (message.success)
            Debug.Log(message.message);
        else 
            Debug.LogError(message.message);
    }
    
    #region Console Commands
    /// <summary>
    /// Console command to check the status of the LLM server.
    /// </summary>
    /// <returns>True if the command was executed.</returns>
    [ConsoleCommand("llmstatus", "Checks the status of the LLM server")]
    public static bool StatusCommand()
    {
        Instance.Status(statusData => 
                Debug.Log($"LLM Server status: healthy: {statusData.healthy}\nLoaded models: {string.Join("\n", statusData.models)}"),
            error => 
                Debug.LogError(error));
        return true;
    }
    
    #endregion
}