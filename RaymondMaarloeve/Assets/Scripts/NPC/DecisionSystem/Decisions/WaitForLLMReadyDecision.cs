public class WaitForLLMReadyDecision : IDecision
{
    public void Setup(IDecisionSystem system, NPC npc)
    {
    }

    public bool Tick()
    {
        return !GameManager.Instance.LlmServerReady;
    }
}