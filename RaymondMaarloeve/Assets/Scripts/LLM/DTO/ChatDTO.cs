using System;
using System.Collections.Generic;

[Serializable]
public class ChatRequestDTO
{
    public string model_id;
    public List<Message> messages;
    public int n_ctx;
    public bool f16_kv;
    public int n_parts;
    public int seed;
    public int n_gpu_layers;
    public int max_tokens;
    public float temperature;
    public float top_p;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatResponseDTO
{
    public string response;
    public float generation_time;
    public int total_tokens;
    public bool success;
}
