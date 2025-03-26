using UnityEngine;
using UnityEngine.AI;

public class WalkDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    public float wanderRadius = 20f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        SelectNewDestination();
    }

    public bool Tick()
    {
        if (!npc.agent.pathPending && npc.agent.remainingDistance < 0.5f)
        {
            return finished;
        }
        return !finished;
    }

    void SelectNewDestination()
    {
        Debug.Log(npc.npcName + ": Choosing new destination!");

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0f;
        randomDirection += npc.transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            npc.agent.SetDestination(hit.position);
            Debug.Log(npc.npcName + ": New destination: " + hit.position);
        }
        else
        {
            Debug.LogWarning(npc.npcName + ": Not found point on NavMesh!");
        }
    }
}
