using UnityEngine;

public class CurrentEnvironment
{
    public IDecision decision;
    public GameObject associatedGameObject;

    public CurrentEnvironment(IDecision decision, GameObject associatedGameObject)
    {
        this.decision = decision;
        this.associatedGameObject = associatedGameObject;
    }
    
    public CurrentEnvironmentDTO ToDTO(NPC npc) => new CurrentEnvironmentDTO(decision.PrettyName, associatedGameObject == null ? 0 : (int) Vector3.Distance(npc.gameObject.transform.position, associatedGameObject.transform.position));
}
