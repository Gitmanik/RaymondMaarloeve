using System;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// Represents a request to calculate the relevance of a new memory based on core and obtained memories.
/// This DTO is used for prompting the LLM to evaluate how relevant a new memory is in the context of an NPC's decision-making process.
/// </summary>
public class CalculateRelevanceDTO
{
    /// <summary>
    /// A list of core memories that are fundamental to the NPC's decision-making process.
    /// </summary>
    public List<string> core_memories;
    /// <summary>
    /// A list of memories that the NPC has obtained, which may influence its current decisions.
    /// </summary>
    public List<ObtainedMemoryDTO> obtained_memories;
    /// <summary>
    /// The new memory that needs to be evaluated for relevance against the core and obtained memories.
    /// </summary>
    public string new_memory;
}
