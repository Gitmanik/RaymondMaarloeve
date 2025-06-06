using UnityEngine;
using UnityEngine.AI;

public class PrayDecision : IDecision
{
    private NPC npc;
    private bool finished = false;
    private bool reachedChurch = false;
    private Vector3 destination;
    private float stoppingDistance = 0.5f;

    public float prayerDuration = 5f;
    private float prayerTimer = 0f;

    public string PrettyName => "praying";
    
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
                Debug.Log(npc.NpcName + ": Going to the church: " + destination);
            }
            else
            {
                Debug.LogWarning(npc.NpcName + ": NavMesh point for the church not found!");
                finished = true;
            }
        }
        else
        {
            Debug.LogWarning(npc.NpcName + ": Church object not found!");
            finished = true;
        }
    }

    public bool Tick()
    {
        if (finished)
            return false;

        if (!reachedChurch)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < stoppingDistance)
            {
                reachedChurch = true;
            }
            return true;
        }
        else
        {
            prayerTimer += Time.deltaTime;
            if (prayerTimer >= prayerDuration)
            {
                finished = true;
            }
            return !finished;
        }
    }
}
