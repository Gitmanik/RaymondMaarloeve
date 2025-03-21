using UnityEngine;

public class GetWaterDecision : IDecision
{
    private NPC npc;
    // TODO well coordinates are generated procedurally 
    private Vector3 wellPosition = new Vector3(15f, 0f, 20f);
    private float stoppingDistance = 0.1f;

    private bool started = false;
    private bool reachedWell = false;

    private float waterCollectionDuration = 4f;
    private float waterCollectionTimer = 0f;

    public void Setup(IDecisionSystem system)
    {
        if (system is RandomDecisionMaker rdm)
        {
            npc = rdm.GetNPC();
        }
    }

    public bool Tick()
    {
        if (npc == null)
            return false;

        if (!started)
        {
            started = true;
            Debug.Log($"{npc.npcName} is going to the well to get water");
        }

        if (!reachedWell)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, wellPosition, npc.speed * Time.deltaTime);
            if (Vector3.Distance(npc.transform.position, wellPosition) <= stoppingDistance)
            {
                reachedWell = true;
                Debug.Log($"{npc.npcName} got to the well");
            }
            return true;
        }
        else
        {
            waterCollectionTimer += Time.deltaTime;
            if (waterCollectionTimer < waterCollectionDuration)
            {
                Debug.Log($"{npc.npcName} takes water from the well");
                return true;
            }
            else
            {
                Debug.Log($"{npc.npcName} finished getting water from the well");
                return false;
            }
        }
    }
}
