using UnityEngine;
using UnityEngine.AI;

public class NPCConversationDecision : IDecision
{
    private NPC npc;
    private NPC partner;
    private bool finished = false;
    public float conversationDistance = 1.5f;
    public float conversationDuration = 5f;
    private float conversationTimer = 0f;
    private bool reachedPartner = false;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        partner = FindConversationPartner();
        if (partner != null)
        {
            npc.agent.SetDestination(partner.transform.position);
            Debug.Log(npc.npcName + ": Going to chat with " + partner.npcName);
        }
        else
        {
            finished = true;
            Debug.Log(npc.npcName + ": Conversation partner not found!");
        }
    }

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
            {
                finished = true;
            }
        }
        return !finished;
    }

    private NPC FindConversationPartner()
    {
        NPC[] allNPCs = GameObject.FindObjectsOfType<NPC>();
        foreach (NPC candidate in allNPCs)
        {
            if (candidate != npc)
                return candidate;
        }
        return null;
    }
}
