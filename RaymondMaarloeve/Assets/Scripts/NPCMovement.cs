using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCWalker : MonoBehaviour
{
    
    public float wanderRadius = 20f;
    public float waitTime = 2f;

    private NavMeshAgent agent;
    private bool going = false;

    void Start()
    {
        

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = 3.5f;
        agent.acceleration = 8f;
        agent.angularSpeed = 120f;

        SelectNewDestination();
    }

    void Update()
    {
        if (going && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            going = false;
            StartCoroutine(CzekajINowyCel());
        }
    }

    IEnumerator CzekajINowyCel()
    {
        yield return new WaitForSeconds(waitTime);
        SelectNewDestination();
    }

    void SelectNewDestination() //Trzeba bêdzie zmieniæ t¹ funkcjê na dok³adne obiekty albo cos
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            going = true;
        }
    }
}
