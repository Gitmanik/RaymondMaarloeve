using UnityEngine;

/// <summary>
/// Represents a decision for an NPC to visit a building to get ale and eat.
/// </summary>
public class GetAleDecision : VisitBuildingDecision
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAleDecision"/> class.
    /// </summary>
    /// <param name="buildingGO">The GameObject representing the building.</param>
    /// <param name="npc">The NPC making the decision.</param>
    public GetAleDecision(GameObject buildingGO, NPC npc) : base(buildingGO, npc)
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
    protected override float WaitDuration => 5f;

    /// <summary>
    /// Human-readable name of the decision.
    /// </summary>
    public override string PrettyName => "getting ale and eating";

    /// <summary>
    /// Called when the decision is finished. Resets the NPC's thirst and hunger.
    /// </summary>
    protected override void OnFinished()
    {
        npc.Thirst = 0f;
        npc.Hunger = 0f;
    }

    /// <summary>
    /// Empty override for the ShouldFinish method.
    /// </summary>
    /// <returns>False, indicating the decision should not finish.</returns>
    protected override bool ShouldFinish() => false;
}
