using UnityEngine;

public class PrayDecision : VisitBuildingDecision
{
    public PrayDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    protected override float StoppingDistance => 0.5f;
    protected override float WaitDuration => 5f;
    public override string PrettyName => "praying";
    protected override void OnFinished()
    {
    }
}
