/**
 * @file WalkDecision.cs
 * @brief Implements wandering behavior: chooses a random point within a radius and moves the NPC there.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class WalkDecision
 * @brief An IDecision that sends the NPC to a random NavMesh location within a specified radius.
 */
public class WalkDecision : IDecision
{
    /** @brief Reference to the NPC executing this decision. */
    private NPC npc;
    /** @brief Whether the NPC has finished walking. */
    private bool finished = false;
    /** @brief Maximum distance from the NPC’s current position to pick a new destination. */
    public float wanderRadius = 20f;

    /**
     * @brief Initializes this decision with its owner NPC.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will walk to a random point.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        SelectNewDestination();
    }

    /**
     * @brief Called every frame to advance the walking behavior.
     * @returns True while the NPC is en route; false once the walk is complete.
     */
    public bool Tick()
    {
        if (!npc.agent.pathPending && npc.agent.remainingDistance < 0.5f)
        {
            finished = true;
        }
        return !finished;
    }

    /**
     * @brief Picks a new random point on the NavMesh within wanderRadius and sets it as the agent’s destination.
     */
    private void SelectNewDestination()
    {
        Debug.Log($"{npc.NpcName}: Choosing new destination!");

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0f;
        randomDirection += npc.transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            npc.agent.SetDestination(hit.position);
            Debug.Log($"{npc.NpcName}: New destination: {hit.position}");
        }
        else
        {
            Debug.LogWarning($"{npc.NpcName}: Could not find a valid point on NavMesh!");
            finished = true;
        }
    }
}
