using System;

/// <summary>
/// Stub <see cref="IDecisionSystem"/> implementation for debugging purposes
/// </summary>
public class NullDecisionSystem : IDecisionSystem
{
    /// <summary>
    /// Sets up the decision-making system with the provided NPC.
    /// </summary>
    /// <param name="npc">The NPC that will use this decision-making system.</param>
    public void Setup(NPC npc)
    {
    }

    /// <summary>
    /// Returns an IdleDecision with idle time of 99999 seconds.
    /// </summary>
    /// <returns><see cref="IdleDecision"/></returns>
    public IDecision Decide()
    {
        return new IdleDecision(99999f);
    }

    /// <summary>
    /// Stub function calling relevanceFunc delegate with relevance value of 0.
    /// </summary>
    /// <param name="newMemory">Not used</param>
    /// <param name="relevanceFunc">Delegate which will be called.</param>
    public void CalculateRelevance(string newMemory, Action<int> relevanceFunc)
    {
        relevanceFunc(0);
    }
}
