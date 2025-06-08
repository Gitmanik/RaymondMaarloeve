/**
 * @file PrayDecision.cs
 * @brief Sends the NPC to a church to pray for a set time.
 */

using UnityEngine;
using UnityEngine.AI;

/**
 * @class PrayDecision
 * @brief An IDecision that moves the NPC to a church and simulates prayer.
 */
public class PrayDecision : IDecision
{
    /** @brief The NPC executing this decision. */
    private NPC npc;
    /** @brief Whether the prayer has finished. */
    private bool finished = false;
    /** @brief Whether the NPC has arrived at the church. */
    private bool reachedChurch = false;
    /** @brief Prayer destination position. */
    private Vector3 destination;
    /** @brief How close the NPC must get to the church to start praying. */
    private float stoppingDistance = 0.5f;
    /** @brief Duration of the prayer once the NPC arrives. */
    public float prayerDuration = 5f;
    private float prayerTimer = 0f;

    /**
     * @brief Finds the church in the scene and moves the NPC there.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will pray.
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
        GameObject churchObj = GameObject.Find("church(Clone)");
        if (churchObj != null)
        {
            Vector3 churchPosition = churchObj.transform.position;
            if (NavMesh.SamplePosition(churchPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                npc.agent.SetDestination(destination);
                Debug.Log($"{npc.NpcName}: Going to the church at {destination}");
            }
            else
            {
                finished = true;
                Debug.LogWarning($"{npc.NpcName}: Could not sample NavMesh at church!");
            }
        }
        else
        {
            finished = true;
            Debug.LogWarning($"{npc.NpcName}: Church object not found!");
        }
    }

    /**
     * @brief Advances movement or prayer each frame.
     * @returns True while moving or praying; false once prayer is complete.
     */
    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedChurch)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
                reachedChurch = true;
            return true;
        }
        else
        {
            prayerTimer += Time.deltaTime;
            if (prayerTimer >= prayerDuration)
                finished = true;
            return !finished;
        }
    }
}
