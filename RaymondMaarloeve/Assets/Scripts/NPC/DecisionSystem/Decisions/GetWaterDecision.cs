/**
 * @file GetWaterDecision.cs
 * @brief Guides an NPC to fetch water from a well and refill its thirst meter.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class GetWaterDecision
 * @brief Moves the NPC to the nearest well, waits to collect water, then resets thirst.
 * 
 * Implements the IDecision interface.
 */
public class GetWaterDecision : IDecision
{
    private NPC npc;                        /**< The NPC performing this action. */
    private bool finished = false;         /**< Whether the action is complete. */
    private bool reachedWell = false;      /**< Whether the NPC has arrived at the well. */
    private Vector3 destination;           /**< Target position at the well. */
    private float stoppingDistance = 0.5f; /**< How close the agent must get to be "at" the well. */

    public float waterCollectionDuration = 4f; /**< How long drinking takes. */
    private float waterCollectionTimer = 0f;    /**< Internal timer for drinking. */

    /**
     * @brief Prepares the NPC to go fetch water.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will move to the well.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject wellObj = GameObject.Find("well(Clone)");
        if (wellObj != null)
        {
            Vector3 wellPosition = wellObj.transform.position;
            if (NavMesh.SamplePosition(wellPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log($"{npc.NpcName}: Going to the well to get water at {destination}");
            }
            else
            {
                Debug.LogWarning($"{npc.NpcName}: NavMesh sample for well not found");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning($"{npc.NpcName}: Well object not found");
            finished = true;
        }
    }

    /**
     * @brief Called every frame to progress the water-fetching action.
     * @returns True if still processing; false when finished.
     */
    public bool Tick()
    {
        if (finished) return false;

        if (!reachedWell)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedWell = true;
            }
            return true;
        }
        else
        {
            waterCollectionTimer += Time.deltaTime;
            if (waterCollectionTimer >= waterCollectionDuration)
            {
                npc.Thirst = 0f;
                finished = true;
            }
            return !finished;
        }
    }
}
