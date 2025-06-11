public interface IDecision
{
    public void Start();
    public void Finish();
    public bool Tick();
    public string PrettyName { get; }
    public string DebugInfo();
}