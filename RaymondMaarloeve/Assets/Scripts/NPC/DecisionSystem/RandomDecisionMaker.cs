using System;
using Random = UnityEngine.Random;

public class RandomDecisionMaker : IDecisionSystem
{
    private NPC npc;

    /// <summary>
    /// Sets up the decision-making system with the provided NPC.
    /// </summary>
    /// <param name="npc">The NPC that will use this decision-making system.</param>
    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

    /// <summary>
    /// Randomly selects the NPC's next action.
    /// </summary>
    /// <returns>An implementation of <see cref="IDecision"/> representing the NPC's next action.</returns>
    public IDecision Decide()
    {
        int decision = Random.Range(0, 5);

        switch (decision)
        {
            case 0:
                return new IdleDecision();
            case 1:
                return new WalkDecision();
            case 2:
                return new GetWaterDecision();
            case 3:
                return new PrayDecision();
            case 4:
                return new GetAleDecision();
            default:
                return new IdleDecision();
        }
    }

    public void CalculateRelevance(string newMemory, Action<int> relevanceFunc)
    {
        relevanceFunc.Invoke(Random.Range(0, 10));
    }
}
