public class WaitForLLMDecision : IDecision
{
    public bool Ready = false;
    
    public string PrettyName => IdleDecision.RandomPrettyName;
    
    public void Setup(IDecisionSystem system, NPC npc)
    {
    }

    public bool Tick()
    {
        return !Ready;
    }
}
