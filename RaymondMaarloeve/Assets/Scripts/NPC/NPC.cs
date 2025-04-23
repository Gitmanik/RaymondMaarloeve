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
