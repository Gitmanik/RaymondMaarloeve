using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a Non-Player Character (NPC) in the game world.
/// Handles decision making, vision, memory, and interaction logic for the NPC.
/// </summary>
public class NPC : MonoBehaviour
{
    /// <summary>
    /// The current target the NPC is looking at.
    /// </summary>
    private Transform lookTarget;
    /// <summary>
    /// Stores the previous rotation before looking at a target.
    /// </summary>
    private Vector3 oldLookTarget;

    /// <summary>
    /// The current decision being executed by the NPC.
    /// </summary>
    private IDecision currentDecision;
    /// <summary>
    /// The decision system used by the NPC.
    /// </summary>
    private IDecisionSystem decisionSystem;
    /// <summary>
    /// The NavMeshAgent component for navigation.
    /// </summary>
    public NavMeshAgent agent;
    /// <summary>
    /// The building associated with this NPC.
    /// </summary>
    public GameObject HisBuilding = null;
    /// <summary>
    /// The Animator component for controlling animations.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// The system prompt used for LLM-based decision making.
    /// </summary>
    public string SystemPrompt { get; private set; } = null;
    /// <summary>
    /// The model ID used for LLM-based decision making.
    /// </summary>
    public string ModelID { get; private set; } = null;
    /// <summary>
    /// The name of the NPC.
    /// </summary>
    public string NpcName { get; private set; } = null;

    /// <summary>
    /// The list of memories obtained by the NPC.
    /// </summary>
    public List<ObtainedMemoryDTO> ObtainedMemories { get; private set; } = new List<ObtainedMemoryDTO>();

    /// <summary>
    /// The hunger level of the NPC.
    /// </summary>
    public float Hunger = 0f;
    /// <summary>
    /// The thirst level of the NPC.
    /// </summary>
    public float Thirst = 0f;

    /// <summary>
    /// The movement speed of the NPC.
    /// </summary>
    public float speed = 3f;
    /// <summary>
    /// The unique entity ID of the NPC.
    /// </summary>
    public int EntityID { get; private set; }

    /// <summary>
    /// The list of NPCs currently visible to this NPC.
    /// </summary>
    private List<NPC> visibleNpcs = new List<NPC>();
    /// <summary>
    /// The cooldown time between vision updates.
    /// </summary>
    private float visionUpdateCooldown = 1f;
    /// <summary>
    /// The timer for vision updates.
    /// </summary>
    private float visionTimer = 0f;

    /// <summary>
    /// The radius of the NPC's field of view.
    /// </summary>
    private float viewRadius = 4f;
    /// <summary>
    /// The angle of the NPC's field of view.
    /// </summary>
    private float viewAngle = 90f;

    /// <summary>
    /// Stores the last observed actions of other NPCs by their entity ID.
    /// </summary>
    private Dictionary<int, string> lastObservedActions = new Dictionary<int, string>();

    /// <summary>
    /// Initializes the NPC, sets up the entity ID, animator, and subscribes to the NPC event bus.
    /// </summary>
    public void Awake()
    {
        EntityID = GameManager.Instance.GetEntityID();

        animator = GetComponent<Animator>();

        NpcEventBus.OnNpcAction += OnNpcActionObserved;
    }
    /// <summary>
    /// Initializes the NavMeshAgent component.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Sets up the NPC's decision system, model, name, and system prompt. Initializes default memories.
    /// </summary>
    /// <param name="decisionSystem">The decision system to use.</param>
    /// <param name="modelId">The model ID for LLM-based decision making.</param>
    /// <param name="name">The name of the NPC.</param>
    /// <param name="systemPrompt">The system prompt for LLM-based decision making.</param>
    public void Setup(IDecisionSystem decisionSystem, string modelId, string name, string systemPrompt)
    {
        this.decisionSystem = decisionSystem;
        decisionSystem.Setup(this);

        ModelID = modelId;
        SystemPrompt = systemPrompt;
        NpcName = name;

        name = "NPC: " + NpcName;
    }

    /// <summary>
    /// Gets the current decision being executed by the NPC.
    /// </summary>
    /// <returns>The current decision.</returns>
    public IDecision GetCurrentDecision() => currentDecision;
    /// <summary>
    /// Gets the decision system used by the NPC.
    /// </summary>
    /// <returns>The decision system.</returns>
    public IDecisionSystem GetDecisionSystem() => decisionSystem;

    /// <summary>
    /// Updates the NPC's state every frame, including animation, decision making, hunger/thirst, and vision.
    /// </summary>
    void Update()
    {
        if (animator != null && agent != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isWalking);
        }

        if (lookTarget != null)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.eulerAngles.y - 180, transform.eulerAngles.z);
            currentDecision = null;
            agent.ResetPath();
        }
        else

        if (currentDecision == null || !currentDecision.Tick())
        {
            Debug.Log($"{NpcName}: Current decision finished");
            currentDecision = decisionSystem.Decide();
            currentDecision.Start();
            Debug.Log($"New decision: {currentDecision.DebugInfo()}");
            NpcEventBus.Publish(new NpcActionEvent(
                sourceId: EntityID,
                action: currentDecision?.PrettyName ?? IdleDecision.RandomPrettyName,
                position: transform.position
            ));
        }

        Hunger += Time.deltaTime * 0.5f;
        Thirst += Time.deltaTime * 0.5f;

        visionTimer += Time.deltaTime;
        if (visionTimer >= visionUpdateCooldown)
        {
            UpdateVision();
            visionTimer = 0f;
        }
    }

    /// <summary>
    /// Updates the list of visible NPCs within the vision cone.
    /// Performs a sphere overlap to find nearby NPCs, checks if they are within the field of view,
    /// and updates the visibleNpcs list. Also tracks observed actions for memory and debugging.
    /// </summary>
    void UpdateVision()
    {
        visibleNpcs.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("NPC"))
            {
                NPC npc = hitCollider.GetComponent<NPC>();
                if (npc != null && npc != this)
                {
                    Vector3 dirToTarget = (npc.transform.position - transform.position).normalized;
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
                    {
                        if (!visibleNpcs.Contains(npc))
                        {
                            string currentAction = npc.currentDecision?.PrettyName ?? IdleDecision.RandomPrettyName;

                            // Check if we have already observed this NPC doing the same action
                            if (!lastObservedActions.ContainsKey(npc.EntityID) || lastObservedActions[npc.EntityID] != currentAction)
                            {
                                lastObservedActions[npc.EntityID] = currentAction;

                                string newMemory = $"Saw {npc.NpcName} doing {currentAction}"; //TODO add hour
            
                                decisionSystem.CalculateRelevance(newMemory, relevance =>
                                {
                                    ObtainedMemories.Add(new ObtainedMemoryDTO
                                    {
                                        memory = newMemory,
                                        weight = relevance + 5 + 10, // TODO: Hard-coded importance = 5, recency = 10
                                    });
                                    Debug.Log($"{NpcName} immediately observed {npc.NpcName} doing {currentAction}. Assigned relevance value: {relevance}");
                                });
                                
                            }
                            visibleNpcs.Add(npc);
                        }
                    }
                }
            }
        }


    }

    /// <summary>
    /// Handles the observation of actions performed by other NPCs.
    /// Ignores actions performed by itself, updates the last observed actions for the observed NPC,
    /// and logs the observation for debugging purposes.
    /// </summary>
    /// <param name="e">The NpcActionEvent containing information about the observed action.</param>
    void OnNpcActionObserved(NpcActionEvent e)
    {
        if (e.SourceId == EntityID) return; // dont observe own actions

        var observedNpc = visibleNpcs.Find(npc => npc.EntityID == e.SourceId);
        if (observedNpc != null)
        {
            lastObservedActions[observedNpc.EntityID] = e.Action;

            string newMemory = $"Saw {observedNpc.NpcName} doing {e.Action}"; //TODO add hour
            
            decisionSystem.CalculateRelevance(newMemory, relevance =>
            {
                ObtainedMemories.Add(new ObtainedMemoryDTO
                {
                    memory = newMemory,
                    weight = relevance + 5 + 10, // TODO: Hard-coded importance = 5, recency = 10
                });

                Debug.Log($"{NpcName} observed {observedNpc.NpcName} doing {e.Action}. Assigned relevance value: {relevance}");
            });
        }
    }



    /// <summary>
    /// Makes the NPC look at a specified target transform.
    /// Updates the NPC's rotation to face the target. If the target is null, resets the rotation to the previous value.
    /// Logs the action for debugging purposes.
    /// </summary>
    /// <param name="targetTransform">The transform of the target to look at. If null, the NPC resets its rotation.</param>
    public void LookAt(Transform targetTransform)
    {
        Debug.Log($"{NpcName} Looking at {(targetTransform == null ? "null" : targetTransform.name)}");
        if (targetTransform != null)
        {
            oldLookTarget = transform.eulerAngles;
            lookTarget = targetTransform;
        }
        else
        {
            transform.eulerAngles = oldLookTarget;
            oldLookTarget = Vector3.zero;
            lookTarget = null;
        }
    }

    /// <summary>
    /// Returns a list of possible environment descriptions for the NPC.
    /// </summary>
    /// <returns>List of CurrentEnvironment objects describing the environment.</returns>
    public List<CurrentEnvironment> GetCurrentEnvironment()
    {
        List<CurrentEnvironment> environments = new List<CurrentEnvironment>();
        
        foreach (var buildingGO in MapGenerator.Instance.spawnedBuildings)
        {
            var buildingData = buildingGO.GetComponent<BuildingData>();
            if (buildingData == null)
            {
                Debug.LogWarning($"{buildingGO.name} has no building data but it was spawned by MapGenerator");
                continue;
            }

            switch (buildingData.HisType)
            {
                case BuildingData.BuildingType.None:
                    break;
                case BuildingData.BuildingType.House:
                    break;
                case BuildingData.BuildingType.Church:
                    environments.Add(new CurrentEnvironment(new PrayDecision(buildingGO, this), buildingGO));
                    break;
                case BuildingData.BuildingType.Well:
                    environments.Add(new CurrentEnvironment(new GetWaterDecision(buildingGO, this), buildingGO));
                    break;
                case BuildingData.BuildingType.Blacksmith:
                    break;
                case BuildingData.BuildingType.Tavern:
                    environments.Add(new CurrentEnvironment(new GetAleDecision(buildingGO, this), buildingGO));
                    break;
                case BuildingData.BuildingType.Scaffold:
                    break;
                case BuildingData.BuildingType.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        environments.Add(new CurrentEnvironment(new IdleDecision(), null));
        environments.Add(new CurrentEnvironment(new WalkDecision(this), null));
        
        return environments;
    }

    /// <summary>
    /// Draws debug lines for the NPC's vision cone in the scene view.
    /// </summary>
    void LateUpdate()
    {
        // Debug lines for vision cone
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Debug.DrawLine(transform.position, transform.position + leftBoundary * viewRadius, Color.cyan);
        Debug.DrawLine(transform.position, transform.position + rightBoundary * viewRadius, Color.cyan);

        Debug.DrawLine(transform.position, transform.position + transform.forward * viewRadius, Color.yellow);
    }
    
    /// <summary>
    /// Unsubscribes from the NPC event bus when the NPC is destroyed.
    /// </summary>
    public void OnDestroy()
    {
        NpcEventBus.OnNpcAction -= OnNpcActionObserved;
    }

}

/// <summary>
/// Represents an event describing an action performed by an NPC.
/// </summary>
public class NpcActionEvent
{
    /// <summary>
    /// The source NPC's entity ID.
    /// </summary>
    public int SourceId;
    /// <summary>
    /// The action performed by the NPC.
    /// </summary>
    public string Action;
    /// <summary>
    /// The position where the action was performed.
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// The timestamp when the event was created.
    /// </summary>
    public float Timestamp;
    
    /// <summary>
    /// Constructs a new NpcActionEvent.
    /// </summary>
    /// <param name="sourceId">The source NPC's entity ID.</param>
    /// <param name="action">The action performed.</param>
    /// <param name="position">The position of the action.</param>
    public NpcActionEvent(int sourceId, string action, Vector3 position)
    {
        SourceId = sourceId;
        Action = action;
        Position = position;
        Timestamp = Time.time;
    }
}

/// <summary>
/// Event bus for publishing and subscribing to NPC action events.
/// </summary>
public static class NpcEventBus
{
    /// <summary>
    /// Event triggered when an NPC action occurs.
    /// </summary>
    public static event Action<NpcActionEvent> OnNpcAction;

    /// <summary>
    /// Publishes an NPC action event to all subscribers.
    /// </summary>
    /// <param name="e">The event to publish.</param>
    public static void Publish(NpcActionEvent e)
    {
        OnNpcAction?.Invoke(e);
    }
}
