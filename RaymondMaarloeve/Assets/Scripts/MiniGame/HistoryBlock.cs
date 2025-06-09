using UnityEngine;

/// <summary>
/// Represents a history fragment block used in the mini-game.
/// Contains a flag indicating whether this block is a genuine fragment or a decoy.
/// </summary>
public class HistoryBlock : MonoBehaviour
{
    /// <summary>
    /// <c>true</c> if this block is a real fragment of history; <c>false</c> if it is a fake (decoy) fragment.
    /// </summary>
    public bool isReal;
}
