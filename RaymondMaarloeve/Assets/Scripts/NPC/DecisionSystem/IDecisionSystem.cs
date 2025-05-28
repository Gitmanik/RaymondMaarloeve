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
    /// Returns the name of the NPC.
    /// </summary>
    /// <returns>The name of the NPC.</returns>
    public string GetNPCName();
}