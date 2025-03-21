using System;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private Transform lookTarget;
    private Vector3 oldLookTarget;
    
    private IDecision currentDecision;
    private IDecisionSystem decisionSystem;

    public string npcName = "Unnamed NPC";

    public float speed = 3f;

    public void Setup(IDecisionSystem decisionSystem)
    {
        this.decisionSystem = decisionSystem;
        decisionSystem.Setup(this);

        npcName = decisionSystem.GetNPCName();
        name = "NPC: " + npcName;
    }
    
    void Update()
    {
        if (lookTarget != null)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.eulerAngles.y - 180, transform.eulerAngles.z);
        }

        if (currentDecision == null || !currentDecision.Tick())
        {
            Debug.Log($"{npcName}: Current decision finished");
            currentDecision = decisionSystem.Decide();
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
