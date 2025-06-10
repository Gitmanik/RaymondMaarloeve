/// <summary>
/// Represents a memory obtained by an NPC in the decision-making system.
/// Each memory has a string identifier and parameters that indicates its importance, recency and relevance.
/// </summary>
public class ObtainedMemory
{
    /// <summary>
    /// The unique identifier for the memory, such as "saw a predator", "found food", etc.
    /// </summary>
    public string memory;

    /// <summary>
    /// Recency of this memory (0-10)
    /// </summary>
    public float recency;
    
    /// <summary>
    /// Relevance of this memory (0-10)
    /// </summary>
    public float relevance;
    
    /// <summary>
    /// Importance from this memory (0-10)
    /// </summary>
    public float importance;

    /// <summary>
    /// Multiplier used when calculating Weight, will be scaled every in-game hour by <see cref="NPC::DecayMemories"/>
    /// </summary>
    public float multiplier = 1f;
    
    public float Weight => (recency + relevance + importance) * multiplier;
    
    /// <summary>
    /// Returns an <see cref="ObtainedMemoryDTO"/> object for use in LLM inference
    /// Weight will be rounded to integer for better LLM inference
    /// </summary>
    /// <returns><see cref="ObtainedMemoryDTO"/></returns>
    public ObtainedMemoryDTO ToDTO() => new ObtainedMemoryDTO() {memory = memory, weight = (int) Weight};
    
    /// <summary>
    /// ToString override used for pretty printing
    /// </summary>
    /// <returns>A pretty print representation of the ObtainedMemory object.</returns>
    public override string ToString() => $"memory: {memory}, relevance: {relevance}, importance: {importance}, recency: {recency}, weight: {Weight}";
}
