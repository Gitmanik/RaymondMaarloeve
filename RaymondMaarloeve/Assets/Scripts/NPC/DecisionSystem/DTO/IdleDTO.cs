using System;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// Represents the idle state of an NPC in the decision-making system.
/// This state includes core memories, obtained memories, current environment, needs, and the stopped action.
/// This DTO is used for prompting the LLM to understand the NPC's current context.
/// </summary>
public class IdleDTO
{
    /// <summary>
    /// A list of core memories that the NPC has, which are fundamental to its decision-making process.
    /// </summary>
    public List<string> core_memories;

    /// <summary>
    /// A list of memories that the NPC has obtained, which may influence its current decisions.
    /// </summary>
    public List<ObtainedMemoryDTO> obtained_memories;

    /// <summary>
    /// A list of current environment objects that the NPC is aware of, which NPC will choose from when making decisions.
    /// </summary>
    public List<CurrentEnvironmentDTO> current_environment;

    /// <summary>
    /// A list of needs that the NPC has, which are used to prioritize actions and decisions.
    /// </summary>
    public List<NeedDTO> needs;

    /// <summary>
    /// The action that the NPC was stopped from performing, which may be relevant for decision-making.
    /// </summary>
    public string stopped_action;
}
