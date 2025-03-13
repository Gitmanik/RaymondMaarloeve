public interface IDecision
{
    public void Setup(IDecisionSystem system);
    public bool Tick();
}