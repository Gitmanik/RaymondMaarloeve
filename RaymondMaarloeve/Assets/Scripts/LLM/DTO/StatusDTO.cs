using System;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// Represents the status of the system, including health and available models.
/// </summary>
public class StatusDTO
{
    /// <summary>
    /// Indicates whether the system is healthy.
    /// </summary>
    public bool healthy;
    /// <summary>
    /// A list of available models in the system.
    /// </summary>
    public List<string> models;
}
