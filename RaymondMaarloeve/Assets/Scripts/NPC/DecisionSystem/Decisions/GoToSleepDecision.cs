using UnityEngine;

public class GoToSleepDecision : VisitBuildingDecision
{
    public GoToSleepDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    protected override float StoppingDistance => 0.5f;
    protected override bool NpcShouldDisappear => true;
    protected override float WaitDuration => 2000f;
    public override string PrettyName => "Going to sleep";
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
        npc.Hunger = 0f;
    }

    protected override bool ShouldFinish() => DayNightCycle.Instance.timeOfDay > 7.5f && DayNightCycle.Instance.timeOfDay < 8.5f;
}
