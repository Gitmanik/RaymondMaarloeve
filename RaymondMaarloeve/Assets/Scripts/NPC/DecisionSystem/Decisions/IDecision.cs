public interface IDecision
{
    public void Setup(IDecisionSystem system, NPC npc);
    public bool Tick();
    public string PrettyName { get; }
}