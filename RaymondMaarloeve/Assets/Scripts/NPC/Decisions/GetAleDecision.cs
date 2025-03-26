using UnityEngine;
using UnityEngine.AI;

public class GetAleDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    private bool reachedTavern = false;
    private Vector3 destination;
    private float stoppingDistance = 0.5f;

    public float drinkingDuration = 5f;
    private float drinkingTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject tavernObj = GameObject.Find("tavern(Clone)");
        if (tavernObj != null)
        {
            Vector3 tavernPosition = tavernObj.transform.position;
            if (NavMesh.SamplePosition(tavernPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log(npc.npcName + ": Going to the tavern: " + destination);
            }
            else
            {
                Debug.LogWarning(npc.npcName + ": NavMesh point for the tavern not found!");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning(npc.npcName + ": Tavern object not found!");
            finished = true;
        }
    }

    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedTavern)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedTavern = true;
            }
            return true;
        }
        else
        {
            drinkingTimer += Time.deltaTime;
            if (drinkingTimer >= drinkingDuration)
            {
                finished = true;
            }
            return !finished;
        }
    }
}
