using UnityEngine;

/// <summary>
/// Represents a single current environment option of an NPC, including the decision to be made and the associated game object.
/// </summary>
public class CurrentEnvironment
{
    /// <summary>
    /// The decision to be made in this action.
    /// </summary>
    public IDecision decision;

    /// <summary>
    /// The game object associated with this environment option.
    /// This could be an object that the NPC interacts with or observes, such as a door, item, or another character.
    /// </summary>
    public GameObject associatedGameObject;


    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentEnvironment"/> class.
    /// </summary>
    /// <param name="decision"></param>
    /// <param name="associatedGameObject"></param>
    public CurrentEnvironment(IDecision decision, GameObject associatedGameObject)
    {
        this.decision = decision;
        this.associatedGameObject = associatedGameObject;
    }

    /// <summary>
    /// Converts the current environment to a <see cref="CurrentEnvironmentDTO"/> DTO representation.
    /// /// This DTO includes the decision's pretty name and the distance to the associated game object from the NPC's position.
    /// The distance is calculated as the Euclidean distance between the NPC's position and the associated game object's position.
    /// If the associated game object is null, the distance is set to 0.
    /// </summary>
    /// <param name="npc">Owner of this object</param>
    /// <returns><see cref="CurrentEnvironmentDTO"/> object </returns>
    public CurrentEnvironmentDTO ToDTO(NPC npc) => new CurrentEnvironmentDTO(decision.PrettyName, associatedGameObject == null ? 0 : (int)Vector3.Distance(npc.gameObject.transform.position, associatedGameObject.transform.position));
}
