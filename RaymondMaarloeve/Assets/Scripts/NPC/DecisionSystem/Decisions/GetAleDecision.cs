/**
 * @file GetAleDecision.cs
 * @brief Directs an NPC to a tavern to drink ale and resets thirst afterward.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class GetAleDecision
 * @brief Moves the NPC to a tavern, simulates drinking, and clears thirst.
 * 
 * Implements the IDecision interface.
 */
public class GetAleDecision : IDecision
{
    private NPC npc;                          /**< The NPC performing this action. */
    private bool finished = false;           /**< Whether the action is complete. */
    private bool reachedTavern = false;      /**< Whether the NPC has arrived at the tavern. */
    private Vector3 destination;             /**< Target position at the tavern. */
    private float stoppingDistance = 0.5f;   /**< How close the agent must get to be "at" the tavern. */

    public float drinkingDuration = 5f; /**< How long the drinking takes. */
    private float drinkingTimer = 0f;   /**< Internal timer for drinking. */

    /**
     * @brief Prepares the NPC to go drink ale.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will move to the tavern.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;

        GameObject tavernObj = GameObject.Find("tavern(Clone)");
        if (tavernObj != null)
        {
            Vector3 tavernPosition = tavernObj.transform.position;
            if (NavMesh.SamplePosition(tavernPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log($"{npc.NpcName}: Going to the tavern at {destination}");
            }
            else
            {
                Debug.LogWarning($"{npc.NpcName}: NavMesh sample for tavern not found");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning($"{npc.NpcName}: Tavern object not found");
            finished = true;
        }
    }

    /**
     * @brief Called each frame to progress the drinking action.
     * @returns True if still drinking; false when done.
     */
    public bool Tick()
    {
        if (finished) return false;

        if (!reachedTavern)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedTavern = true;
            }
            return true;
        }
        else
        {
            drinkingTimer += Time.deltaTime;
            if (drinkingTimer >= drinkingDuration)
            {
                npc.Thirst = 0f;
                finished = true;
            }
            return !finished;
        }
    }
}
