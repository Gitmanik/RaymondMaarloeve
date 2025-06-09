using System;

[Serializable]
public class ObtainedMemoryDTO
{
    public string memory;
    public float weight;
    public override string ToString() => $"memory: {memory}, weight: {weight}";
}