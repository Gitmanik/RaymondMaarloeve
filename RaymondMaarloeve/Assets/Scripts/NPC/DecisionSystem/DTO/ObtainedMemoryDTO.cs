using System;

[Serializable]
public class ObtainedMemoryDTO
{
    public string memory;
    public int weight;
    public override string ToString() => $"memory: {memory}, weight: {weight}";
}