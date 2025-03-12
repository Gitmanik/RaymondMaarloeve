using UnityEngine;

public class NPC : MonoBehaviour
{
    private Transform lookTarget;
    private Vector3 oldLookTarget;
    
    void Update()
    {
        if (lookTarget != null)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.eulerAngles.y - 180, transform.eulerAngles.z);
        }
    }
    
    public void LookAt(Transform targetTransform)
    {
        Debug.Log("Looking at " + (targetTransform == null ? "null" :  targetTransform.name));
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
