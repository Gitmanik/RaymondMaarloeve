using System;
using System.Collections.Generic;

[Serializable]
public class IdleDTO
{
    public List<string> core_memories;
    public List<ObtainedMemoryDTO> obtained_memories;
    public List<CurrentEnvironmentDTO> current_environment;
    public List<NeedDTO> needs;
    public string stopped_action;
}