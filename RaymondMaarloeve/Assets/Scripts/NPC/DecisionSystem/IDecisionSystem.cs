using System;

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

    public void CalculateRelevance(string newMemory, Action<int> relevanceFunc);
}