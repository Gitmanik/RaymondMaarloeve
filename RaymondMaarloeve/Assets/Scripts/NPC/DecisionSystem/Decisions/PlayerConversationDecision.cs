using UnityEngine;
using UnityEngine.AI;

public class PlayerConversationDecision : IDecision
{
    private NPC npc;
    private Transform playerTransform;
    private bool finished = false;
    private bool reachedPlayer = false;

    public float conversationDistance = 1.5f;

    public float conversationDuration = 5f;
    private float conversationTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;

            Vector3 playerPos = playerTransform.position;
            if (NavMesh.SamplePosition(playerPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                npc.agent.SetDestination(hit.position);
                Debug.Log(npc.NpcName + ": Going to meet Player: " + hit.position);
            }

        }
        else
        {
            Debug.LogWarning(npc.NpcName + ": Player object not found!");
            finished = true;
        }
    }

    public bool Tick()
    {
        if (finished)
            return false;

        if (playerTransform == null)
        {
            finished = true;
            return false;
        }

        if (!npc.agent.pathPending)
        {
            npc.agent.SetDestination(playerTransform.position);
        }

        if (!reachedPlayer)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < conversationDistance)
            {
                reachedPlayer = true;
            }
            return true;
        }
        else
        {
            conversationTimer += Time.deltaTime;
            if (conversationTimer >= conversationDuration)
            {
                finished = true;
            }
            return !finished;
        }
    }
}
