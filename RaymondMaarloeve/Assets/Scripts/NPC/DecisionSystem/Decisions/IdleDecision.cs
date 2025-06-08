/**
 * @file IdleDecision.cs
 * @brief Implements idle behavior: NPC stands still for a random duration.
 */

using UnityEngine;

/**
 * @class IdleDecision
 * @brief An IDecision that makes the NPC wait (idle) for a random period before proceeding.
 */
public class IdleDecision : IDecision
{
    /** @brief The time when idling started. */
    private float idleStart;
    /** @brief The randomly chosen duration to remain idle. */
    private float idleTime;

    /**
     * @brief Initializes the idle timer.
     * @param system The decision system invoking this decision (unused).
     * @param npc The NPC that will idle (unused).
     */
    public void Setup(IDecisionSystem system, NPC npc)
    {
        idleStart = Time.time;
        idleTime = Random.Range(0f, 15f);
    }

    /**
     * @brief Called each frame to check if the idle period has elapsed.
     * @returns True after idling is complete; false while still idling.
     */
    public bool Tick()
    {
        return Time.time < idleStart + idleTime;
    }
}
