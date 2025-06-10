using UnityEngine;

public class GetAleDecision : VisitBuildingDecision
{
    public GetAleDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    protected override float StoppingDistance => 0.5f;
    protected override bool NpcShouldDisappear => true;
    protected override float WaitDuration => 5f;
    public override string PrettyName => "getting ale";
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
    }
}
