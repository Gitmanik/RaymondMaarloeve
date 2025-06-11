public class WaitForLLMReadyDecision : IDecision
{
    public string PrettyName => IdleDecision.RandomPrettyName;
    public string DebugInfo() => "waiting for LLMServer";

    public void Start() {}
    public void Finish() {}

    public bool Tick()
    {
        return !GameManager.Instance.LlmServerReady;
    }
}