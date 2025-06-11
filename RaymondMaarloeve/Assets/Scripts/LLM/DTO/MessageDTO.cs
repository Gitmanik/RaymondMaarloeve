using System;

[Serializable]
/// <summary>
/// Represents a response message of the LLMServer.
/// </summary>
public class MessageDTO
{
    /// <summary>
    /// Whether the action was successful or not.
    /// </summary>
    public bool success;

    /// <summary>
    /// The message content returned by the server.
    /// </summary>
    public string message;
}
