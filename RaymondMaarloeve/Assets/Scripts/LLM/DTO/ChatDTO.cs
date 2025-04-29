using System;
using System.Collections.Generic;

[Serializable]
public class ChatRequestDTO
{
    public string model_id;
    public List<Message> messages;
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
