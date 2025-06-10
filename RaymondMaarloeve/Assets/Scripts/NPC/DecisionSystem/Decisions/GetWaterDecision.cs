using UnityEngine;

public class GetWaterDecision : VisitBuildingDecision
{
    public GetWaterDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    protected override float StoppingDistance => 2f;
    protected override bool NpcShouldDisappear => false;
    protected override float WaitDuration => 4f;
    public override string PrettyName => "going to get water";
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
    }
}
