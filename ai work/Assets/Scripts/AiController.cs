using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AiController : MonoBehaviour
{
    public NavMeshAgent agent; //your agent

    [SerializeField] public GameObject playerRef; //target to chase

    //fov part
    public float radius;
    [Range(0, 360)]
    public float angle;

    //add layer in the inspector to choose the layer
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public LayerMask groundMask;

    //smell hoooman?
    public bool detectPlayer;
    public bool playerInAttackRange;

    //Patroling
    public float range; //radius of sphere

    public Transform centrePoint; //centre of the area the agent wants to move around in
    //instead of centrePoint you can set it as the transform of the agent if you don't care about a specific area

    //Chase

    //Attack
    public float attackRange;

    private void Start()
    {
        StartCoroutine(FOVRoutine());
    }
    //When game is awake get the element
    private void Awake()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
    }

    //make sure ai doesnt check for player every fps it will cause lag
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        //lets look if we had find everything
        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            //angle check
            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    detectPlayer = true;
                    //action when player been spotted add here
                    Debug.Log("you been spotted");
                }
                else
                    detectPlayer = false;
            }
            else
            {
                detectPlayer = false;
            }
        }
    }

    private void Update()
    {
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, targetMask);
        //expect ai will routing
        if (!detectPlayer)
            Patroling();

        //expect ai will chase
        if (detectPlayer && !playerInAttackRange)
            ChasePlayer();

        //expect ai will standing and attack
        if (detectPlayer && playerInAttackRange)
            AttackPlayer();
    }

    void Patroling()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) //done with path
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point)) //pass in our centre point and radius of area
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                agent.SetDestination(point);
            }
        }
    }
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
    void ChasePlayer()
    {
        agent.SetDestination(playerRef.transform.position);
    }
    void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);
        transform.LookAt(playerRef.transform);
        Debug.Log("Fire!");

    }
}