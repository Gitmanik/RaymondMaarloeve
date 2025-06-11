using System;

[Serializable]
/// <summary>
/// Represents the <see cref="CurrentEnvironment"/> DTO object used for prompting the LLM.
/// This object contains the action to be performed and the distance to the target.
/// </summary>
public class CurrentEnvironmentDTO
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentEnvironmentDTO"/> class.
    /// </summary>
    /// <param name="action">Action to perform</param>
    /// <param name="distance">Euclidean distance</param>
    public CurrentEnvironmentDTO(string action, int distance)
    {
        this.action = action;
        this.distance = distance;
    }

    /// <summary>
    /// The action to be performed in the current environment.
    /// </summary>
    public string action;
    
    /// <summary>
    /// Euclidean distance to the target in the current environment.
    /// </summary>
    public int distance;
}
