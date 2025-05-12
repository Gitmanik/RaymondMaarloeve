using UnityEngine;
using UnityEngine.AI;

public class GetWaterDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    private bool reachedWell = false;
    private Vector3 destination;
    private float stoppingDistance = 0.5f;

    public float waterCollectionDuration = 4f;
    private float waterCollectionTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject wellObj = GameObject.Find("well(Clone)");
        if (wellObj != null)
        {
            Vector3 wellPosition = wellObj.transform.position;
            if (NavMesh.SamplePosition(wellPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log(npc.npcName + ": Going to the well to get water: " + destination);
            }
            else
            {
                Debug.LogWarning(npc.npcName + ": NavMesh point for the well not found!");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning(npc.npcName + ": Well object not found!");
            finished = true;
        }
    }

    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedWell)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedWell = true;
            }
            return true;
        }
        else
        {
            waterCollectionTimer += Time.deltaTime;
            if (waterCollectionTimer >= waterCollectionDuration)
            {
                finished = true;
            }
            return !finished;
        }
    }
}
