using System;

[Serializable]

/// <summary>
/// DTO class for <see cref="NPC::DrawConclusions"> LLM inference
/// </summary>
public class DrawConclusionsResponseDTO
{
    /// <summary>
    /// Generated paragraph
    /// </summary>
    public string paragraph;
    /// <summary>
    /// Selected action, 1 if None
    /// </summary>
    public int action;
}
