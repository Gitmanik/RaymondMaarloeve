public class WaitForLLMDecision : IDecision
{
    public bool Ready = false;
    
    public void Setup(IDecisionSystem system, NPC npc)
    {
    }

    public bool Tick()
    {
        return !Ready;
    }
}
