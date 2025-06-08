using UnityEngine;
using UnityEngine.AI;

public abstract class VisitBuildingDecision : IDecision
{
    private GameObject buildingGO;
    private bool finished;
    private Vector3 destination;
    protected NPC npc;
    private bool reachedBuilding;
    private float waitTimer;

    protected abstract float WaitDuration { get; }
    
    protected abstract float StoppingDistance { get; }

    private VisitBuildingDecision() {}
    
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
            Debug.Log($"Entrance found for building {buildingGO.name} at position {entrance.position}");
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

    public bool Tick()
    {
        if (finished)
        {
            OnFinished();
            return false;
        }

        if (!reachedBuilding)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < StoppingDistance)
            {
                reachedBuilding = true;
            }
            return true;
        }
        
        waitTimer += Time.deltaTime;
        if (waitTimer >= WaitDuration)
        {
            finished = true;
        }
        return !finished;
    }


    public void Start()
    {
        npc.agent.SetDestination(destination);
    }
    
    public string DebugInfo() => $"visiting building at: {destination}, reachedBuilding: {reachedBuilding}, waitDuration: {WaitDuration}, stoppingDistance: {StoppingDistance}";
    public abstract string PrettyName { get; }
    protected abstract void OnFinished();
}
