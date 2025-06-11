/// <summary>
/// Defines the Decision for the NPC Decision System.
/// </summary>
public interface IDecision
{
    /// <summary>
    /// Initializes the decision action. Implementation should begin execution of the decision logic.
    /// </summary>
    public void Start();

    /// <summary>
    /// Finalizes the decision action. Implementation should clean up any resources or state related to the decision.
    /// </summary>
    public void Finish();

    /// <summary>
    /// Executes the decision logic and determines the outcome.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the decision is finished; otherwise, <c>false</c>.
    /// </returns>
    public bool Tick();

    /// <summary>
    /// Gets the human-readable description of the decision.
    /// </summary>
    public string PrettyName { get; }

    /// <summary>
    /// Provides debugging information for the decision.
    /// </summary>
    /// <returns>A string containing debugging information.</returns>
    public string DebugInfo();
}
