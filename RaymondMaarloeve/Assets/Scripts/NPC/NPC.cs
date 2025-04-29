using System;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{

    private Transform lookTarget;
    private Vector3 oldLookTarget;
    
    private IDecision currentDecision;
    private IDecisionSystem decisionSystem;
    public NavMeshAgent agent;
    
    // TODO: Use narrator AI for generating the SystemPrompt
    public string SystemPrompt { get; private set; } = "Your name is Wilfred von Rabenstein. You are a fallen knight, a drunkard, and a man whose name was once spoken with reverence, now drowned in ale and regret. You are 42 years old. You are undesirable in most places, yet your blade still holds value for those desperate enough to hire a ruined man. It is past midnight. You are slumped against the wall of a rundown tavern, the rain mixing with the stale stench of cheap wine on your cloak. You know the filth of the city—the beggars, the whores, the men who whisper in shadows. You drink every night until the world blurs, until the past feels like a dream. You speak with the slurred grace of a man who once addressed kings but now bargains for pennies.";
    public string ModelID { get; private set; } = "tuned-model";

    public string npcName = "Unnamed NPC";

    public float speed = 3f;
    public int EntityID { get; private set; }

    public void Awake()
    {
        EntityID = GameManager.Instance.GetEntityID();
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(IDecisionSystem decisionSystem)
    {
        this.decisionSystem = decisionSystem;
        decisionSystem.Setup(this);

        npcName = decisionSystem.GetNPCName();
        name = "NPC: " + npcName;
    }

    public IDecision GetCurrentDecision() => currentDecision;
    public IDecisionSystem GetDecisionSystem() => decisionSystem;
    
    void Update()
    {
        if (lookTarget != null)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.eulerAngles.y - 180, transform.eulerAngles.z);
            currentDecision = null;
            agent.ResetPath();
        } else

        if (currentDecision == null || !currentDecision.Tick())
        {
            Debug.Log($"{npcName}: Current decision finished");
            currentDecision = decisionSystem.Decide();
            currentDecision.Setup(decisionSystem, this);
            Debug.Log($"New decision: {currentDecision}");
        }

    }

    public void LookAt(Transform targetTransform)
    {
        Debug.Log($"{npcName} Looking at {(targetTransform == null ? "null" :  targetTransform.name)}");
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
} 
