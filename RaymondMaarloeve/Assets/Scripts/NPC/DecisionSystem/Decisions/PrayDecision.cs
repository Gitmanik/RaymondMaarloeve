using UnityEngine;

/// <summary>
/// Represents a decision for an NPC to visit a building and perform a praying action.
/// </summary>
public class PrayDecision : VisitBuildingDecision
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrayDecision"/> class.
    /// </summary>
    /// <param name="buildingGO">The GameObject representing the building to visit.</param>
    /// <param name="npc">The NPC making the decision.</param>
    public PrayDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    /// <summary>
    /// Stopping distance for the NPC when approaching the building.
    /// </summary>
    protected override float StoppingDistance => 1f;

    /// <summary>
    /// Whether the NPC should disappear after completing the decision.
    /// </summary>
    protected override bool NpcShouldDisappear => true;

    /// <summary>
    /// Duration the NPC should wait at the building.
    /// </summary>
    protected override float WaitDuration => 5f;

    /// <summary>
    /// Human-readable name of the decision.
    /// </summary>
    public override string PrettyName => "praying";

    /// <summary>
    /// Called when the decision is finished.
    /// </summary>
    protected override void OnFinished()
    {
    }

    /// <summary>
    /// Determines whether the decision should finish.
    /// </summary>
    /// <returns>Always returns false for this decision.</returns>
    protected override bool ShouldFinish() { return false; }
}
