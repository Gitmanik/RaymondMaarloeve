using System;

[Serializable]
/// <summary>
/// Represents a memory obtained by an NPC in the decision-making system.
/// Each memory has a string identifier and a weight that indicates its importance.
/// This DTO is used for prompting the LLM to understand the NPC's experiences and how they influence its decisions.
/// </summary>
public class ObtainedMemoryDTO
{
    /// <summary>
    /// The unique identifier for the memory, such as "saw a predator", "found food", etc.
    /// </summary>
    public string memory;
    /// <summary>
    /// The weight of the memory, indicating its importance or relevance to the NPC's current situation.
    /// </summary>
    public int weight;

    /// <summary>
    /// ToString override used for pretty printing
    /// </summary>
    /// <returns>A pretty print representation of the ObtainedMemoryDTO object.<
    public override string ToString() => $"memory: {memory}, weight: {weight}";
}
