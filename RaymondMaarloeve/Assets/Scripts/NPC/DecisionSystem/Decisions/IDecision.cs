/**
 * @file IDecision.cs
 * @brief Defines the interface for all NPC decision actions.
 */

/// <summary>
/// Interface that all decision classes must implement.
/// </summary>
public interface IDecision
{
    /**
     * @brief Initializes the decision with the owning system and NPC.
     * @param system The decision system invoking this decision.
     * @param npc The NPC that will perform the decision.
     */
    public void Setup(IDecisionSystem system, NPC npc);

    /**
     * @brief Called each frame to execute or advance the decision.
     * @returns True if the decision is still in progress; false once it has completed.
     */
    public bool Tick();
}
