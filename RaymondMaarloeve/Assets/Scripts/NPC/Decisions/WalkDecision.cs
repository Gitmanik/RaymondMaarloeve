using UnityEngine;
// let's treat this as a placeholder for now as the walk logic doesn't seem to work properly
public class WalkDecision : IDecision
{
    private NPC npc;
    private Vector3 destination;
    private float stoppingDistance = 0.1f;
    private bool started = false;
    private bool finished = false;

    public void Setup(IDecisionSystem system, NPC npc)
    {
        this.npc = npc;
    }

    public bool Tick()
    {
        if (!started && npc != null)
        {
            destination = npc.transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            started = true;
        }

        if (started && !finished)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, destination, npc.speed * Time.deltaTime);

            if (Vector3.Distance(npc.transform.position, destination) <= stoppingDistance)
            {
                finished = true;
            }
        }

        return !finished;
    }
}
