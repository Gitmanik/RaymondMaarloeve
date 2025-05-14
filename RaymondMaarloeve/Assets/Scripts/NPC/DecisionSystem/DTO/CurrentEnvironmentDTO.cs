using System;

[Serializable]
public class CurrentEnvironmentDTO
{
    public CurrentEnvironmentDTO(string action, int distance)
    {
        this.action = action;
        this.distance = distance;
    }

    public string action;
    public int distance;
}