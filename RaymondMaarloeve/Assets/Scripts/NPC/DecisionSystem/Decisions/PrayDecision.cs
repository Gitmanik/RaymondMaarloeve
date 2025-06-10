using UnityEngine;

public class PrayDecision : VisitBuildingDecision
{
    public PrayDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    protected override float StoppingDistance => 1f;
    protected override bool NpcShouldDisappear => true;
    protected override float WaitDuration => 5f;
    public override string PrettyName => "praying";
    protected override void OnFinished()
    {
    }
}
