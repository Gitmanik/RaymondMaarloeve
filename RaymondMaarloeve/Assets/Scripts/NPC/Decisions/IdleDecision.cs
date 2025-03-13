using UnityEngine;

public class IdleDecision : IDecision
{
    private float idleStart;
    private float idleTime;
    
    public void Setup(IDecisionSystem system)
    {
        idleStart = Time.time;
        idleTime = Random.Range(0f, 15f);
    }

    public bool Tick()
    {
        if (idleStart + idleTime > Time.time)
        {
            return false;
        }
        
        return true;
    }
}
