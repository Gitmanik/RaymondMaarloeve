using UnityEngine;
using UnityEngine.AI;

public class BuyGoodsDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    private bool reachedMarket = false;
    private Vector3 destination;
    private float stoppingDistance = 0.5f;

    public float buyingDuration = 4f;
    private float buyingTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        //TODO we don't have market building yet
        GameObject marketObj = GameObject.Find("market (Clone)");
        if (marketObj != null)
        {
            Vector3 marketPosition = marketObj.transform.position;
            if (NavMesh.SamplePosition(marketPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log(npc.npcName + ": Going to the market: " + destination);
            }
            else
            {
                Debug.LogWarning(npc.npcName + ": NavMesh point for the market not found!");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning(npc.npcName + ": Market object not found!");
            finished = true;
        }
    }

    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedMarket)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedMarket = true;
            }
            return true;
        }
        else
        {
            buyingTimer += Time.deltaTime;
            if (buyingTimer >= buyingDuration)
            {
                finished = true;
            }
            return !finished;
        }
    }
}
