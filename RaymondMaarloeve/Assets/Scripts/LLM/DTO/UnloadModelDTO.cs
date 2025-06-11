using System;

[Serializable]
///
/// <summary
/// Represents a request to unload a model from the system.
/// </summary>
public class UnloadModelRequestDTO
{
    /// <summary>
    /// The unique identifier of the model to be unloaded.
    /// </summary>
    public string model_id;
}
