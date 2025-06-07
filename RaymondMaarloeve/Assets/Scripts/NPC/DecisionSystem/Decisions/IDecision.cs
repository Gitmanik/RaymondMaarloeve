public interface IDecision
{
    public void Start();
    public bool Tick();
    public string PrettyName { get; }
    public string DebugInfo();
}