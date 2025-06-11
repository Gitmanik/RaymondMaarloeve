using UnityEngine;

/// <summary>
/// Represents a decision for an NPC to visit a building to get water.
/// </summary>
public class GetWaterDecision : VisitBuildingDecision
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetWaterDecision"/> class.
    /// </summary>
    /// <param name="buildingGO">The GameObject representing the building.</param>
    /// <param name="npc">The NPC making the decision.</param>
    public GetWaterDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
    {
    }

    /// <summary>
    /// Stopping distance for the NPC when approaching the building.
    /// </summary>
    protected override float StoppingDistance => 2f;

    /// <summary>
    /// Whether the NPC should disappear after completing the decision.
    /// </summary>
    protected override bool NpcShouldDisappear => false;

    /// <summary>
    /// Duration the NPC should wait at the building.
    /// </summary>
    protected override float WaitDuration => 4f;

    /// <summary>
    /// Human-readable name of the decision.
    /// </summary>
    public override string PrettyName => "going to get water";

    /// <summary>
    /// Called when the decision is finished. Resets the NPC's thirst level.
    /// </summary>
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
    }

    /// <summary>
    /// Empty override for the ShouldFinish method.
    /// </summary>
    /// <returns>False, indicating the decision should not finish.</returns>
    protected override bool ShouldFinish() => false;
}
