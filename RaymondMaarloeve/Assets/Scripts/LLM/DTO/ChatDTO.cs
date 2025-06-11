using System;
using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for sending chat requests to the LLM.
/// </summary>
[Serializable]
public class ChatRequestDTO
{
    /// <summary>
    /// The identifier of the model to use.
    /// </summary>
    public string model_id;

    /// <summary>
    /// The list of <see cref="Message"> in the conversation.
    /// </summary>
    public List<Message> messages;

    /// <summary>
    /// The context window size for model.
    /// </summary>
    public int n_ctx;

    /// <summary>
    /// Whether to use 16-bit key/value memory.
    /// </summary>
    public bool f16_kv;

    /// <summary>
    /// The number of parts to split the model into.
    /// </summary>
    public int n_parts;

    /// <summary>
    /// The random seed for generation.
    /// </summary>
    public int seed;

    /// <summary>
    /// The number of GPU layers to use.
    /// If set to -1, the model will use all available GPU layers.
    /// If set to 0, the model will run on CPU.
    /// If set to a positive integer, it will use that many GPU layers.
    /// </summary>
    public int n_gpu_layers;

    /// <summary>
    /// The maximum number of tokens to generate.
    /// </summary>
    public int max_tokens;

    /// <summary>
    /// The temperature for sampling.
    /// </summary>
    public float temperature;

    /// <summary>
    /// The nucleus sampling probability.
    /// </summary>
    public float top_p;
}

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
[Serializable]
public class Message
{
    /// <summary>
    /// The role of the message sender (e.g., "user", "assistant").
    /// </summary>
    public string role;

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string content;
}

/// <summary>
/// Data Transfer Object for receiving chat responses from the LLM.
/// </summary>
[Serializable]
public class ChatResponseDTO
{
    /// <summary>
    /// The generated response text.
    /// </summary>
    public string response;

    /// <summary>
    /// The time taken to generate the response, in seconds.
    /// </summary>
    public float generation_time;

    /// <summary>
    /// The total number of tokens used in the response.
    /// </summary>
    public int total_tokens;

    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool success;
}
