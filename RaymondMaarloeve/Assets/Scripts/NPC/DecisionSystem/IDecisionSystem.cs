public interface IDecisionSystem
{
    public void Setup(NPC npc);
    public IDecision Decide();
    public string GetNPCName();
}