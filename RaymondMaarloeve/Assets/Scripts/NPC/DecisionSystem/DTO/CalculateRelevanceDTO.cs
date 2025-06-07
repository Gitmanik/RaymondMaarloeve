using System;
using System.Collections.Generic;

[Serializable]
public class CalculateRelevanceDTO
{
    public List<string> core_memories;
    public List<ObtainedMemoryDTO> obtained_memories;
    public string new_memory;
}