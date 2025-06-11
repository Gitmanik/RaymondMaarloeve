using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a decision for an NPC to idle for a specified duration.
/// </summary>
public class IdleDecision : IDecision
{
    /// <summary>
    /// The time when the idling started.
    /// </summary>
    private float idleStart;

    /// <summary>
    /// The duration of the idling in seconds.
    /// </summary>
    private float idleTime = -1f;

    /// <summary>
    /// A list of random idle names for descriptive purposes.
    /// </summary>
    public static readonly List<string> RandomIdleNames = new List<string>()
    {
        // "loitering",
        // "lazing",
        // "lingering",
        // "dawdling",
        // "dallying",
        // "resting",
        "hanging around",
        // "vegetating",
        // "malingering",
        // "chilling",
        // "biding time",
        // "stalling"
    };

    /// <summary>
    /// Gets a random idle name from the list of idle names.
    /// </summary>
    public static string RandomPrettyName => RandomIdleNames[Random.Range(0, RandomIdleNames.Count)];

    /// <summary>
    /// Human-readable name of the decision.
    /// </summary>
    public string PrettyName => RandomPrettyName;

    /// <summary>
    /// Provides debug information about the remaining idle time.
    /// </summary>
    /// <returns>A string containing the remaining idle time.</returns>
    public string DebugInfo() => $"idling for another {idleStart + idleTime - Time.time} seconds";

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleDecision"/> class with a specified idle time.
    /// </summary>
    /// <param name="idleTime">The duration of the idling in seconds.</param>
    public IdleDecision(float idleTime)
    {
        this.idleTime = idleTime;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleDecision"/> class with a random idle time.
    /// </summary>
    public IdleDecision()
    {
        this.idleTime = Random.Range(0f, 15f);
    }

    /// <summary>
    /// Starts the idle decision by recording the current time.
    /// </summary>
    public void Start()
    {
        idleStart = Time.time;
    }

    /// <summary>
    /// Called when the decision finishes. Currently, no implementation is provided.
    /// </summary>
    public void Finish()
    {
    }

    /// <summary>
    /// Updates the idle decision and checks if the idle time has elapsed.
    /// </summary>
    /// <returns><c>true</c> if the NPC should continue idling; otherwise, <c>false</c>.</returns>
    public bool Tick()
    {
        if (idleStart + idleTime > Time.time)
        {
            return true;
        }

        return false;
    }
}
