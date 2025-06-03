using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    private Transform lookTarget;
    private Vector3 oldLookTarget;

    private IDecision currentDecision;
    private IDecisionSystem decisionSystem;
    public NavMeshAgent agent;

    //public string npcName = "Unnamed NPC";
    public GameObject HisBuilding = null;
    private Animator animator;

    public string SystemPrompt { get; private set; } = null;
    public string ModelID { get; private set; } = null;
    public string NpcName { get; private set; } = null;

    public List<ObtainedMemoryDTO> ObtainedMemories { get; private set; } = new List<ObtainedMemoryDTO>();

    public float Hunger = 0f;
    public float Thirst = 0f;

    public float speed = 3f;
    public int EntityID { get; private set; }

    private List<NPC> visibleNpcs = new List<NPC>();
    private float visionUpdateCooldown = 1f;
    private float visionTimer = 0f;

    private float viewRadius = 4f;
    private float viewAngle = 90f;

    private Dictionary<int, string> lastObservedActions = new Dictionary<int, string>();


    public void Awake()
    {
        EntityID = GameManager.Instance.GetEntityID();

        animator = GetComponent<Animator>();

        NpcEventBus.OnNpcAction += OnNpcActionObserved;
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(IDecisionSystem decisionSystem, string modelId, string name, string systemPrompt)
    {
        this.decisionSystem = decisionSystem;
        decisionSystem.Setup(this);

        ModelID = modelId;
        SystemPrompt = systemPrompt;
        NpcName = name;

        name = "NPC: " + NpcName;

        // TODO: Make this dynamic.
        ObtainedMemories = new List<ObtainedMemoryDTO>() {
            new ObtainedMemoryDTO
            {
                memory = "A child asked what the sigil on my ring was. I told him it meant 'nothing'.",
                weight = 21
            },
            new ObtainedMemoryDTO
            {
                memory = "A merchant mentioned my house name, then spat. I didn’t blink.",
                weight = 20
            },
            new ObtainedMemoryDTO
            {
                memory = "Saw my old banner being used to wrap fish. I said nothing. But I watched.",
                weight = 23
            },
            new ObtainedMemoryDTO
            {
                memory = "Heard someone say I should’ve been executed too. He’s not wrong.",
                weight = 22
            },
            new ObtainedMemoryDTO
            {
                memory = "A drunken man bowed to me by mistake. For a moment, I let him.",
                weight = 18
            },
            new ObtainedMemoryDTO
            {
                memory = "Someone asked if I’d return home. I said ‘Home is gone.’",
                weight = 19
            }
        };
    }

    public IDecision GetCurrentDecision() => currentDecision;
    public IDecisionSystem GetDecisionSystem() => decisionSystem;

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
            currentDecision.Setup(decisionSystem, this);
            Debug.Log($"New decision: {currentDecision}");
            NpcEventBus.Publish(new NpcActionEvent(
                sourceId: EntityID,
                action: currentDecision?.ToString() ?? "Idle",
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
                            string currentAction = npc.currentDecision?.ToString() ?? "Idle";

                            // Check if we have already observed this NPC doing the same action
                            if (!lastObservedActions.ContainsKey(npc.EntityID) || lastObservedActions[npc.EntityID] != currentAction)
                            {
                                lastObservedActions[npc.EntityID] = currentAction;

                                // ObtainedMemories.Add(new ObtainedMemoryDTO
                                // {
                                //     memory = $"Saw {npc.NpcName} doing {currentAction} at {npc.transform.position}",
                                //     weight = UnityEngine.Random.Range(10, 25)
                                // });

                                Debug.Log($"{NpcName} immediately observed {npc.NpcName} doing {currentAction}");
                            }
                            visibleNpcs.Add(npc);
                        }
                    }
                }
            }
        }


    }

    void OnNpcActionObserved(NpcActionEvent e)
    {
        if (e.SourceId == EntityID) return; // dont observe own actions

        var observedNpc = visibleNpcs.Find(npc => npc.EntityID == e.SourceId);
        if (observedNpc != null)
        {
            lastObservedActions[observedNpc.EntityID] = observedNpc.currentDecision?.ToString() ?? "Idle";
            // ObtainedMemories.Add(new ObtainedMemoryDTO
            // {
            //     memory = $"Saw {observedNpc.NpcName} doing {e.Action} at {e.Position}",
            //     weight = UnityEngine.Random.Range(10, 25)
            // });

            Debug.Log($"{NpcName} observed {observedNpc.NpcName} doing {e.Action}.");
        }
    }



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

    public List<CurrentEnvironmentDTO> GetCurrentEnvironment()
    {
        //TODO: Make this dynamic.
        return new List<CurrentEnvironmentDTO>()
        {
            new CurrentEnvironmentDTO("Stand by the chapel steps, unmoving (pray)", 2),
            new CurrentEnvironmentDTO("Sit upright beneath a broken statue (walk)", 1),
            new CurrentEnvironmentDTO("Gaze at the river without blinking (idle)", 3),
            new CurrentEnvironmentDTO("Walk slowly through the square without speaking (walk)", 2),
            new CurrentEnvironmentDTO("Trace the burned symbol on his ring (walk)", 1),
            new CurrentEnvironmentDTO("Watch birds scatter in the market (buy goods)", 2)
        };
    }

    void LateUpdate()
    {
        // Debug lines for vision cone
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Debug.DrawLine(transform.position, transform.position + leftBoundary * viewRadius, Color.cyan);
        Debug.DrawLine(transform.position, transform.position + rightBoundary * viewRadius, Color.cyan);

        Debug.DrawLine(transform.position, transform.position + transform.forward * viewRadius, Color.yellow);
    }
    
    public void OnDestroy()
    {
        NpcEventBus.OnNpcAction -= OnNpcActionObserved;
    }

}

public class NpcActionEvent
{
    public int SourceId;
    public string Action;
    public Vector3 Position;
    public float Timestamp;
    
    public NpcActionEvent(int sourceId, string action, Vector3 position)
    {
        SourceId = sourceId;
        Action = action;
        Position = position;
        Timestamp = Time.time;
    }
}

public static class NpcEventBus
{
    public static event Action<NpcActionEvent> OnNpcAction;

    public static void Publish(NpcActionEvent e)
    {
        OnNpcAction?.Invoke(e);
    }
}
