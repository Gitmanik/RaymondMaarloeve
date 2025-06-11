/// <summary>
/// Represents a decision that waits for a LLM-generated decision.
/// </summary>
public class WaitForLLMDecision : IDecision
{
    /// <summary>
    /// Indicates whether the decision is ready to proceed.
    /// </summary>
    public bool Ready = false;

    /// <summary>
    /// Gets a pretty name for the decision, sourced from <see cref="IdleDecision"/>.
    /// </summary>
    public string PrettyName => IdleDecision.RandomPrettyName;

    /// <summary>
    /// Provides debug information about the current state of the decision.
    /// </summary>
    /// <returns>"waiting for LLM Decision"</returns>
    public string DebugInfo() => "waiting for LLM Decision";

    /// <summary>
    /// Empty implementation for the Start method.
    /// </summary>
    public void Start() {}

    /// <summary>
    /// Empty implementation for the Finish method.
    /// </summary>
    public void Finish() {}

    /// <summary>
    /// Updates the decision logic. Called periodically to check the decision's state.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the decision is not ready; otherwise, <c>false</c>.
    /// </returns>
    public bool Tick()
    {
        return !Ready;
    }
}
