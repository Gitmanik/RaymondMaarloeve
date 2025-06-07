public class WaitForLLMDecision : IDecision
{
    public bool Ready = false;
    
    public string PrettyName => IdleDecision.RandomPrettyName;
    public string DebugInfo() => "waiting for LLM Decision";

    public void Start() {}

    public bool Tick()
    {
        return !Ready;
    }
}
