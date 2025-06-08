/**
 * @file WaitForLLMReadyDecision.cs
 * @brief A decision that waits for the LLM server to become available.
 */

/// <summary>
/// Decision that delays NPC actions until the game’s LLM server is reported ready.
/// </summary>
public class WaitForLLMReadyDecision : IDecision
{
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
     * @brief Continues waiting until GameManager.Instance.LlmServerReady is true.
     * @returns True while the server is not ready; false once it is ready.
     */
    public bool Tick()
    {
        return !GameManager.Instance.LlmServerReady;
    }
}
