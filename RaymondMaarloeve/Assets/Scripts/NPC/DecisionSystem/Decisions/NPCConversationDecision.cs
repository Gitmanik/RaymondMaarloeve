/**
 * @file NPCConversationDecision.cs
 * @brief Sends the NPC to speak with another NPC for a set duration.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class NPCConversationDecision
 * @brief An IDecision that moves the NPC to the nearest other NPC and holds a conversation.
 */
public class NPCConversationDecision : IDecision
{
    /** @brief The NPC executing this decision. */
    private NPC npc;
    /** @brief The conversation partner NPC. */
    private NPC partner;
    /** @brief Whether the conversation has finished. */
    private bool finished = false;
    /** @brief Distance within which the NPC is considered “at” the partner. */
    public float conversationDistance = 1.5f;
    /** @brief How long the conversation lasts once started. */
    public float conversationDuration = 5f;
    private float conversationTimer = 0f;
    private bool reachedPartner = false;

    /**
     * @brief Finds a partner and moves the NPC to them.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will converse.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        partner = FindConversationPartner();
        if (partner != null)
        {
            npc.agent.SetDestination(partner.transform.position);
            Debug.Log($"{npc.NpcName}: Going to chat with {partner.NpcName}");
        }
        else
        {
            finished = true;
            Debug.LogWarning($"{npc.NpcName}: Conversation partner not found!");
        }
    }

    /**
     * @brief Advances movement or conversation each frame.
     * @returns True while moving or conversing; false when done.
     */
    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedPartner)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < conversationDistance)
            {
                reachedPartner = true;
            }
        }
        else
        {
            conversationTimer += Time.deltaTime;
            if (conversationTimer >= conversationDuration)
                finished = true;
        }
        return !finished;
    }

    /**
     * @brief Locates any other NPC in the scene to talk to.
     * @returns The first found NPC that is not the caller; null if none exist.
     */
    private NPC FindConversationPartner()
    {
        foreach (NPC candidate in GameObject.FindObjectsOfType<NPC>())
        {
            if (candidate != npc)
                return candidate;
        }
        return null;
    }
}
