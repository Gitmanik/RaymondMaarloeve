using UnityEngine;

public class PlayerConversationDecision : IDecision
{
    private NPC npc;
    private Transform playerTransform;

    private float conversationDistance = 1.5f;

    private float conversationDuration = 5f;
    private float conversationTimer = 0f;

    private bool started = false;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject playerObj = GameObject.FindWithTag("player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    public bool Tick()
    {
        if (npc == null || playerTransform == null)
            return false;

        if (!started)
        {
            started = true;
            Debug.Log($"{npc.npcName} decided to engage in conversation with Raymond Maarloeve");
        }

        float distance = Vector3.Distance(npc.transform.position, playerTransform.position);
        if (distance > conversationDistance)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, playerTransform.position, npc.speed * Time.deltaTime);
            return true;
        }
        else
        {
            //placeholder for actual interaction
            conversationTimer += Time.deltaTime;
            if (conversationTimer < conversationDuration)
            {
                Debug.Log($"{npc.npcName} chats with our Raymond...");
                return true;
            }
            else
            {
                Debug.Log($"{npc.npcName} ended chat with Raymond");
                return false;
            }
        }
    }
}
