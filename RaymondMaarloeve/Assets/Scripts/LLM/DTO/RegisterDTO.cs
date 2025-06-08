using System;

/// <summary>
/// Data Transfer Object for registering a model with the LLM server.
/// Contains the model's unique identifier and the file path to the model on disk.
/// </summary>
[Serializable]
public class RegisterDTO
{
    /// <summary>
    /// The unique identifier for the model.
    /// </summary>
    public string model_id;

    /// <summary>
    /// The file path to the model on disk.
    /// </summary>
    public string model_path;
}
