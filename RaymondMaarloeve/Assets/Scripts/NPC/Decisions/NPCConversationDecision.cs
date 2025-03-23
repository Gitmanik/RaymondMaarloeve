using UnityEngine;

public class NPCConversationDecision : IDecision
{
    private NPC npc;              
    private NPC partner;          // conversation partner
    private bool partnerFound = false;

    private float conversationDistance = 1.5f;

    private float conversationDuration = 5f;
    private float conversationTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
    }

    public bool Tick()
    {
        if (npc == null)
            return false;

        if (!partnerFound)
        {
            partner = FindConversationPartner();
            if (partner != null)
            {
                partnerFound = true;
                Debug.Log($"{npc.npcName} will engage in conversation with: {partner.npcName}");
            }
            else
            {
                Debug.Log($"{npc.npcName} didn't find conversation partner");
                return false; 
            }
        }

        float distance = Vector3.Distance(npc.transform.position, partner.transform.position);
        if (distance > conversationDistance)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, partner.transform.position, npc.speed * Time.deltaTime);
            return true;
        }
        else
        {
            conversationTimer += Time.deltaTime;
            if (conversationTimer < conversationDuration)
            {
                Debug.Log($"{npc.npcName} chats with {partner.npcName}...");
                return true;
            }
            else
            {
                Debug.Log($"{npc.npcName} ended chat with {partner.npcName}.");
                return false;
            }
        }
    }

    private NPC FindConversationPartner()
    {
        NPC[] allNPCs = GameObject.FindObjectsOfType<NPC>();
        foreach (var candidate in allNPCs)
        {
            if (candidate != npc)
            {
                return candidate;
            }
        }
        return null;
    }
}
