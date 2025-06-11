using System.Collections.Generic;
using UnityEngine;

public class IdleDecision : IDecision
{
    private float idleStart;
    private float idleTime = -1f;

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
    public static string RandomPrettyName => RandomIdleNames[Random.Range(0, RandomIdleNames.Count)];
    public string PrettyName => RandomPrettyName;
    public string DebugInfo() => $"idling for another {idleStart + idleTime - Time.time} seconds";

    public IdleDecision(float idleTime)
    {
        this.idleTime = idleTime;
    }

    public IdleDecision()
    {
        this.idleTime = Random.Range(0f, 15f);
    }
    
    public void Start()
    {
        idleStart = Time.time;
    }

    public void Finish()
    {
    }

    public bool Tick()
    {
        if (idleStart + idleTime > Time.time)
        {
            return true;
        }
        
        return false;
    }
}
