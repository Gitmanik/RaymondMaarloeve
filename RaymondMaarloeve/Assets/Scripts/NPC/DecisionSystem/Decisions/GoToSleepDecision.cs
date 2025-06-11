using UnityEngine;

/// <summary>
/// Represents a decision for an NPC to go to sleep.
/// </summary>
public class GoToSleepDecision : VisitBuildingDecision
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoToSleepDecision"/> class.
    /// </summary>
    /// <param name="buildingGO">The GameObject representing the building the NPC will visit.</param>
    /// <param name="npc">The NPC making the decision.</param>
    public GoToSleepDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    /// <summary>
    /// Stopping distance for the NPC when approaching the building.
    /// </summary>
    protected override float StoppingDistance => 0.5f;

    /// <summary>
    /// Whether the NPC should disappear after completing the decision.
    /// </summary>
    protected override bool NpcShouldDisappear => true;

    /// <summary>
    /// Duration the NPC should wait at the building.
    /// </summary>
    protected override float WaitDuration => 2000f;

    /// <summary>
    /// Human-readable name of the decision.
    /// </summary>
    public override string PrettyName => "Going to sleep";

    /// <summary>
    /// Called when the decision is finished. Resets the NPC's thirst and hunger levels.
    /// </summary>
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
        npc.Hunger = 0f;
    }

    /// <summary>
    /// Determines whether the decision should finish based on the time of the day.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the time of day is between 7:30 and 8:30 (dawn); otherwise, <c>false</c>.
    /// </returns>
    protected override bool ShouldFinish() => DayNightCycle.Instance.timeOfDay > 7.5f && DayNightCycle.Instance.timeOfDay < 8.5f;
}
