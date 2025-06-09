using System;

/// <summary>
/// Represents the configuration for an individual LLM used in the game.
/// This configuration is provided by the game launcher and is part of the JSON structure managed by GameConfig.
/// </summary>
[Serializable]
public class ModelConfig 
{
    /// <summary>
    /// Unique identifier for the model.
    /// </summary>
    public int Id; 
    /// <summary>
    /// Name of the model.
    /// </summary>
    public string Name; 
    /// <summary>
    /// File system path to the model.
    /// </summary>
    public string Path;
}