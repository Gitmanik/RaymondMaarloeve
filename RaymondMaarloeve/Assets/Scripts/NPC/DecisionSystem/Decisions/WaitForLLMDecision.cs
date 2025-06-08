/**
 * @file WaitForLLMDecision.cs
 * @brief A decision that pauses until an LLM response is ready.
 */

/// <summary>
/// Decision that stalls NPC behavior until an external LLM has produced a result.
/// </summary>
public class WaitForLLMDecision : IDecision
{
    /** @brief Flag indicating whether the LLM output is ready. Defaults to false. */
    public bool Ready = false;

    /**
     * @brief No setup required for this decision.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will be paused (unused).
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        // Intentionally left blank.
    }

    /**
     * @brief Continues waiting until Ready becomes true.
     * @returns True while still waiting; false once Ready is true.
     */
    public bool Tick()
    {
        return !Ready;
    }
}
