using UnityEngine;
using UnityEngine.AI;

public class WalkDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    public float wanderRadius = 20f;
    private Vector3 destination;

    public string PrettyName => "walking";
    public string DebugInfo() => $"walking to {destination}";

    public WalkDecision(NPC npc)
    {
        this.npc = npc;
        SelectNewDestination();
    }

    public void Start()
    {
        npc.agent.SetDestination(destination);
    }

    public void Finish()
    {
        npc.agent.ResetPath();
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
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0f;
            randomDirection += npc.transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                destination = hit.position;
                break;
            }
            else
            {
                Debug.LogWarning(npc.NpcName + ": Not found point on NavMesh!");
            }
        }
    }
}
