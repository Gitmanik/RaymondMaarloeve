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
    protected abstract bool NpcShouldDisappear { get; }

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
            if (NpcShouldDisappear)
            {
                npc.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                npc.agent.enabled = true;
            }
            OnFinished();
            return false;
        }

        if (!reachedBuilding)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < StoppingDistance)
            {
                if (NpcShouldDisappear)
                {
                    npc.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                    npc.agent.enabled = false;

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


    public void Start()
    {
        npc.agent.SetDestination(destination);
        npc.agent.stoppingDistance = StoppingDistance;
    }
    
    public string DebugInfo() => $"visiting building {buildingGO.name} at: {destination}, reachedBuilding: {reachedBuilding}, waitDuration: {WaitDuration}, stoppingDistance: {StoppingDistance}";
    public abstract string PrettyName { get; }
    protected abstract void OnFinished();
}
