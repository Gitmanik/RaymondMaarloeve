public class WaitForLLMReadyDecision : IDecision
{
    public string PrettyName => IdleDecision.RandomPrettyName;
    
    public void Setup(IDecisionSystem system, NPC npc)
    {
    }

    public bool Tick()
    {
        return !GameManager.Instance.LlmServerReady;
    }
}