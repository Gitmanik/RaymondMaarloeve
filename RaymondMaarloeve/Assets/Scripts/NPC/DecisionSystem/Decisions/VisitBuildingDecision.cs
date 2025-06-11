using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a decision for an NPC to visit a building.
/// Handles navigation, waiting, and optional disappearance behavior.
/// </summary>
public abstract class VisitBuildingDecision : IDecision
{
    /// <summary>
    /// The GameObject representing the building or its entrance.
    /// </summary>
    private GameObject buildingGO;

    /// <summary>
    /// Indicates whether the decision is finished.
    /// </summary>
    private bool finished;

    /// <summary>
    /// The destination position on the NavMesh for the building.
    /// </summary>
    private Vector3 destination;

    /// <summary>
    /// The NPC associated with this decision.
    /// </summary>
    protected NPC npc;

    /// <summary>
    /// Indicates whether the NPC has reached the building.
    /// </summary>
    public bool reachedBuilding { get; private set; }

    /// <summary>
    /// Timer for tracking the wait duration after reaching the building.
    /// </summary>
    private float waitTimer;

    /// <summary>
    /// The duration the NPC should wait after reaching the building.
    /// </summary>
    protected abstract float WaitDuration { get; }
    
    /// <summary>
    /// The stopping distance for the NPC when approaching the building.
    /// </summary>
    protected abstract float StoppingDistance { get; }

    /// <summary>
    /// Indicates whether the NPC should disappear upon reaching the building.
    /// </summary>
    protected abstract bool NpcShouldDisappear { get; }

    /// <summary>
    /// Private constructor to prevent instantiation without parameters.
    /// </summary>
    private VisitBuildingDecision() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="VisitBuildingDecision"/> class.
    /// </summary>
    /// <param name="buildingGO">The building GameObject.</param>
    /// <param name="npc">The NPC making the decision.</param>
    public VisitBuildingDecision(GameObject buildingGO, NPC npc)
    {
        if (buildingGO == null)
        {
            Debug.LogError("VisitBuildingDecision buildingGO is null!");
            finished = true;
            return;
        }

        this.buildingGO = buildingGO;
        
        Transform entrance = buildingGO.transform.Find("Entrance");
        if (entrance != null)
        {
            this.buildingGO = entrance.gameObject;
        }
        
        this.npc = npc;
        
        if (NavMesh.SamplePosition(buildingGO.transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }
        else
        {
            Debug.LogWarning(npc.NpcName + ": NavMesh point for the building not found!");
            finished = true;
        }
    }

    /// <summary>
    /// Executes the decision logic each frame.
    /// </summary>
    /// <returns>True if the decision is ongoing; false if finished.</returns>
    public bool Tick()
    {
        if (finished || ShouldFinish())
        {
            Finish();
            return false;
        }

        if (!reachedBuilding)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < StoppingDistance)
            {
                if (NpcShouldDisappear)
                {
                    npc.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                    npc.agent.radius = 0.01f;
                }
                reachedBuilding = true;
            }
            return true;
        }
        
        waitTimer += Time.deltaTime;
        if (waitTimer >= WaitDuration)
        {
            finished = true;
        }

        return true;
    }

    /// <summary>
    /// Starts the decision by setting the NPC's destination and stopping distance.
    /// </summary>
    public void Start()
    {
        npc.agent.SetDestination(destination);
        npc.agent.stoppingDistance = StoppingDistance;
    }

    /// <summary>
    /// Finishes the decision and restores the NPC's state if necessary.
    /// </summary>
    public void Finish()
    {
        if (NpcShouldDisappear)
        {
            npc.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            npc.agent.radius = 0.5f;
        }
        OnFinished();
    }
    
    /// <summary>
    /// Provides debug information about the decision.
    /// </summary>
    /// <returns>A string containing debug information.</returns>
    public string DebugInfo() => $"visiting building {(buildingGO.name == "Entrance" ? buildingGO.transform.parent.gameObject.name : buildingGO.name)} at: {destination}, reachedBuilding: {reachedBuilding}, waitDuration: {WaitDuration}, stoppingDistance: {StoppingDistance}";

    /// <summary>
    /// Gets the human-readable name of the decision.
    /// </summary>
    public abstract string PrettyName { get; }

    /// <summary>
    /// Called when the decision is finished.
    /// </summary>
    protected abstract void OnFinished();

    /// <summary>
    /// Determines whether the decision should finish.
    /// </summary>
    /// <returns>True if the decision should finish; otherwise, false.</returns>
    protected abstract bool ShouldFinish();
}
