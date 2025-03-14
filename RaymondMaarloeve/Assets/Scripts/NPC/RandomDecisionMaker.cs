using UnityEngine;

public class RandomDecisionMaker : IDecisionSystem
{
    private NPC npc;

    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

    public IDecision Decide()
    {
        return new IdleDecision();
    }

    public string GetNPCName()
    {
        return "Random NPC name";
    }
}
