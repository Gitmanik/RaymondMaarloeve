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

    private Animator animator;


    // TODO: Use narrator AI for generating the SystemPrompt
    public string SystemPrompt { get; private set; } = "Your name is Wilfred von Rabenstein. You are a fallen knight, a drunkard, and a man whose name was once spoken with reverence, now drowned in ale and regret. You are 42 years old. You are undesirable in most places, yet your blade still holds value for those desperate enough to hire a ruined man. It is past midnight. You are slumped against the wall of a rundown tavern, the rain mixing with the stale stench of cheap wine on your cloak. You know the filth of the city—the beggars, the whores, the men who whisper in shadows. You drink every night until the world blurs, until the past feels like a dream. You speak with the slurred grace of a man who once addressed kings but now bargains for pennies.";
    public string ModelID { get; private set; } = null;
    public string NpcName { get; private set; } = null;
    
    public List<ObtainedMemoryDTO> ObtainedMemories { get; private set; } = new List<ObtainedMemoryDTO>();
    
    public float Hunger = 0f;
    public float Thirst = 0f;

    public float speed = 3f;
    public int EntityID { get; private set; }

    public void Awake()
    {
        EntityID = GameManager.Instance.GetEntityID();

        animator = GetComponent<Animator>();

    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(IDecisionSystem decisionSystem, string modelId, string name)
    {
        this.decisionSystem = decisionSystem;
        decisionSystem.Setup(this);

        ModelID = modelId;
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
        } else

        if (currentDecision == null || !currentDecision.Tick())
        {
            Debug.Log($"{NpcName}: Current decision finished");
            currentDecision = decisionSystem.Decide();
            currentDecision.Setup(decisionSystem, this);
            Debug.Log($"New decision: {currentDecision}");
        }
        
        Hunger += Time.deltaTime * 0.5f;
        Thirst += Time.deltaTime * 0.5f;
    }

    public void LookAt(Transform targetTransform)
    {
        Debug.Log($"{NpcName} Looking at {(targetTransform == null ? "null" :  targetTransform.name)}");
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
} 
