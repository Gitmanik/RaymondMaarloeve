/// <summary>
/// Represents a decision that waits for the LLM server to be ready.
/// </summary>
public class WaitForLLMReadyDecision : IDecision
{
    /// <summary>
    /// Gets a pretty name for the decision, sourced from <see cref="IdleDecision"/>.
    /// </summary>
    public string PrettyName => IdleDecision.RandomPrettyName;

    /// <summary>
    /// Provides debug information about the decision.
    /// </summary>
    /// <returns>A string indicating the decision is waiting for the LLM server.</returns>
    public string DebugInfo() => "waiting for LLMServer";

    /// <summary>
    /// Called when the decision starts. Currently, no implementation is provided.
    /// </summary>
    public void Start() {}

    /// <summary>
    /// Called when the decision finishes. Currently, no implementation is provided.
    /// </summary>
    public void Finish() {}

    /// <summary>
    /// Evaluates the decision logic.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the LLM server is not ready; otherwise, <c>false</c>.
    /// </returns>
    public bool Tick()
    {
        return !GameManager.Instance.LlmServerReady;
    }
}
