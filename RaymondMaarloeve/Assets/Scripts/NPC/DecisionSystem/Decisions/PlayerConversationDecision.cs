/**
 * @file PlayerConversationDecision.cs
 * @brief Directs the NPC to approach the player and chat for a duration.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class PlayerConversationDecision
 * @brief An IDecision that moves the NPC to the player and simulates a conversation.
 */
public class PlayerConversationDecision : IDecision
{
    /** @brief The NPC executing this decision. */
    private NPC npc;
    /** @brief Transform of the player to approach. */
    private Transform playerTransform;
    /** @brief Whether the conversation is complete. */
    private bool finished = false;
    /** @brief Whether the NPC has reached the player. */
    private bool reachedPlayer = false;
    /** @brief Distance at which the NPC is “close enough” to converse. */
    public float conversationDistance = 1.5f;
    /** @brief Duration of the conversation once the NPC arrives. */
    public float conversationDuration = 5f;
    private float conversationTimer = 0f;

    /**
     * @brief Finds the player GameObject and sets destination.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will talk to the player.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            if (NavMesh.SamplePosition(playerTransform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                npc.agent.SetDestination(hit.position);
                Debug.Log($"{npc.NpcName}: Going to meet player at {hit.position}");
            }
        }
        else
        {
            finished = true;
            Debug.LogWarning($"{npc.NpcName}: Player object not found!");
        }
    }

    /**
     * @brief Called each frame to move toward or converse with the player.
     * @returns True while moving or talking; false once conversation ends.
     */
    public bool Tick()
    {
        if (finished)
            return false;

        if (playerTransform == null)
        {
            finished = true;
            return false;
        }

        // Continuously update destination in case the player moves
        if (!npc.agent.pathPending)
            npc.agent.SetDestination(playerTransform.position);

        if (!reachedPlayer)
        {
            if (npc.agent.remainingDistance < conversationDistance)
                reachedPlayer = true;
            return true;
        }
        else
        {
            conversationTimer += Time.deltaTime;
            if (conversationTimer >= conversationDuration)
                finished = true;
            return !finished;
        }
    }
}
