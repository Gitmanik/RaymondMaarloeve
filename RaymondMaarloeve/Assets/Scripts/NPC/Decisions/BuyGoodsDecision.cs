using UnityEngine;

public class BuyGoodsDecision : IDecision
{
    private NPC npc;
    // TODO market stall coordinates are generated procedurally
    private Vector3 stallPosition = new Vector3(10f, 0f, 10f);
    private float stoppingDistance = 0.1f;

    private bool started = false;
    private bool reachedStall = false;

    //npc will stand for a while to simulate buying goods from merchant
    private float interactionDuration = 3f;
    private float interactionTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
    }

    public bool Tick()
    {
        if (npc == null)
            return false;

        if (!started)
        {
            started = true;
        }

        if (!reachedStall)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, stallPosition, npc.speed * Time.deltaTime);
            if (Vector3.Distance(npc.transform.position, stallPosition) <= stoppingDistance)
            {
                reachedStall = true;
            }
        }
        else
        {
            interactionTimer += Time.deltaTime;
            if (interactionTimer >= interactionDuration)
            {
                return false;
            }
        }

        return true;
    }
}
