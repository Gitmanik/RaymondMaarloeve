using System;

/// <summary>
/// Represents the configuration for an individual NPC used in the game.
/// This configuration is provided by the game launcher and is part of the JSON structure managed by GameConfig.
/// </summary>
[Serializable]
public class NpcConfig 
{
    /// <summary>
    /// Unique identifier for the NPC.
    /// </summary>
    public int Id; 
    /// <summary>
    /// ID of the LLM associated with the NPC.
    /// </summary>
    public int ModelId; 
}