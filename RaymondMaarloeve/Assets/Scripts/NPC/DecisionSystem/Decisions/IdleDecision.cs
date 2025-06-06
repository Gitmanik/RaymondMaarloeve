using UnityEngine;

public class IdleDecision : IDecision
{
    private float idleStart;
    private float idleTime = -1f;

    public IdleDecision(float idleTime)
    {
        this.idleTime = idleTime;
    }
    public IdleDecision() {}
    
    public void Setup(IDecisionSystem system, NPC npc)
    {
        idleStart = Time.time;
        if (idleTime < 0)
            idleTime = Random.Range(0f, 15f);
    }

    public bool Tick()
    {
        if (idleStart + idleTime > Time.time)
        {
            return true;
        }
        
        return false;
    }
}
