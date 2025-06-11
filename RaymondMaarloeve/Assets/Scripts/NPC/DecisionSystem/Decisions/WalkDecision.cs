using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a decision for an NPC to walk to a random destination within a specified radius.
/// </summary>
public class WalkDecision : IDecision
{
    /// <summary>
    /// The <see cref="NPC"/> associated with this decision.
    /// </summary>
    private NPC npc;

    /// <summary>
    /// The radius within which the NPC will wander.
    /// </summary>
    public float wanderRadius = 20f;
    
    /// <summary>
    /// The destination position on the NavMesh for the NPC to walk to.
    /// </summary>
    private Vector3 destination;

    /// <summary>
    /// Gets the pretty name of the decision for LLM inference purposes.
    /// </summary>
    public string PrettyName => "walking";

    /// <summary>
    /// Gets debug information about the current destination.
    /// </summary>
    /// <returns>A string containing the current destination.</returns>
    public string DebugInfo() => $"walking to {destination}";

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkDecision"/> class.
    /// </summary>
    /// <param name="npc">The <see cref="NPC"> associated with this decision.</param>
    public WalkDecision(NPC npc)
    {
        this.npc = npc;
        SelectNewDestination();
    }

    /// <summary>
    /// Starts the walking decision by setting the <see cref="NPC"/>'s NavMeshAgent destination.
    /// </summary>
    public void Start()
    {
        npc.agent.SetDestination(destination);
    }

    /// <summary>
    /// Finishes the walking decision by resetting the <see cref="NPC"/>'s NavMeshAgent path.
    /// </summary>
    public void Finish()
    {
        npc.agent.ResetPath();
    }

    /// <summary>
    /// Updates the decision logic. Checks if the NPC has reached its destination.
    /// </summary>
    /// <returns>False if the walk is finished; otherwise, true.</returns>
    public bool Tick()
    {
        if (!npc.agent.pathPending && npc.agent.remainingDistance < 0.5f)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Selects a new random destination within the wander radius.
    /// </summary>
    private void SelectNewDestination()
    {
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0f;
            randomDirection += npc.transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                destination = hit.position;
                break;
            }
            else
            {
                Debug.LogWarning(npc.NpcName + ": Not found point on NavMesh!");
            }
        }
    }
}
