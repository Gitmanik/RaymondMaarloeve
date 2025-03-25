using UnityEngine;

public class PrayDecision : IDecision
{
    private NPC npc;
    // TODO church coords are procedurally generated
    private Vector3 churchPosition = new Vector3(20f, 0f, 15f);
    private float stoppingDistance = 0.1f;

    private bool started = false;
    private bool reachedChurch = false;

    private float prayerDuration = 5f;
    private float prayerTimer = 0f;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
    }

    public bool Tick()
    {
        if (npc == null)
            return false;

        if (!started)
        {
            started = true;
            Debug.Log($"{npc.npcName} is going to the church to pray");
        }

        if (!reachedChurch)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, churchPosition, npc.speed * Time.deltaTime);
            if (Vector3.Distance(npc.transform.position, churchPosition) <= stoppingDistance)
            {
                reachedChurch = true;
                Debug.Log($"{npc.npcName} got to the church");
            }
            return true;
        }
        else
        {
            prayerTimer += Time.deltaTime;
            if (prayerTimer < prayerDuration)
            {
                Debug.Log($"{npc.npcName} is praying...");
                return true;
            }
            else
            {
                Debug.Log($"{npc.npcName} finished praying");
                return false;
            }
        }
    }
}
