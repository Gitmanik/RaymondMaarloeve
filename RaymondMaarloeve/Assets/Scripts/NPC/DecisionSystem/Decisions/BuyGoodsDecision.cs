/**
 * @file BuyGoodsDecision.cs
 * @brief Sends an NPC to the market to buy goods and resets hunger on completion.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class BuyGoodsDecision
 * @brief Moves the NPC to a market location, waits to simulate buying, then clears hunger.
 * 
 * Implements the IDecision interface.
 */
public class BuyGoodsDecision : IDecision
{
    private NPC npc;                         /**< The NPC performing this action. */
    private bool finished = false;          /**< Whether the action is complete. */
    private bool reachedMarket = false;     /**< Whether the NPC has arrived at the market. */
    private Vector3 destination;            /**< Target position at the market. */
    private float stoppingDistance = 0.5f;  /**< How close the agent must get to be "at" the market. */

    public float buyingDuration = 4f; /**< How long the buying process takes. */
    private float buyingTimer = 0f;   /**< Internal timer for buying. */

    /**
     * @brief Prepares the NPC to go buy goods.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will move to the market.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject marketObj = GameObject.Find("market(Clone)");
        if (marketObj != null)
        {
            Vector3 marketPosition = marketObj.transform.position;
            if (NavMesh.SamplePosition(marketPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log($"{npc.NpcName}: Going to the market at {destination}");
            }
            else
            {
                Debug.LogWarning($"{npc.NpcName}: NavMesh sample for market not found");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning($"{npc.NpcName}: Market object not found");
            finished = true;
        }
    }

    /**
     * @brief Called each frame to advance the purchasing action.
     * @returns True if still in progress; false once buying is done.
     */
    public bool Tick()
    {
        if (finished) return false;

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
                npc.Hunger = 0f;
                finished = true;
            }
            return !finished;
        }
    }
}
