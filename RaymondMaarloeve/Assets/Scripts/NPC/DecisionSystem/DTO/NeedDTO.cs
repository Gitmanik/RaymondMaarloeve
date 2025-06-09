using System;

[Serializable]
/// <summary>
/// Represents a need of an NPC in the decision-making system.
/// Each need has a string identifier and a weight that indicates its importance.
/// This DTO is used for prompting the LLM to understand the NPC's needs.
/// </summary>
public class NeedDTO
{
    /// <summary>
    /// The unique identifier for the need, such as "hunger", "thirst", etc.
    /// </summary>
    public string need;
    /// <summary>
    /// The weight of the need, indicating its importance or urgency.
    /// </summary>
    public int weight;
}
