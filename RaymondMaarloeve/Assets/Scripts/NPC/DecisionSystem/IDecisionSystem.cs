using System;

/// <summary>
/// Interface for a decision-making system for NPCs.
/// </summary>
public interface IDecisionSystem
{
    /// <summary>
    /// Sets up the decision-making system with the provided NPC.
    /// </summary>
    /// <param name="npc">The NPC that will use this decision-making system.</param>
    public void Setup(NPC npc);

    /// <summary>
    /// Decides the NPC's next action.
    /// </summary>
    /// <returns>An implementation of <see cref="IDecision"/> representing the NPC's next action.</returns>
    public IDecision Decide();

    /// <summary>
    /// Requests a calculation of relevance from the LLM server based on the current environment and NPC state.
    /// </summary>
    /// <param name="newMemory">New obtained memory to process</param>
    /// <param name="relevanceFunc">Delegate which will be called when the value is calculated.</param>
    public void CalculateRelevance(string newMemory, Action<int> relevanceFunc);
}
