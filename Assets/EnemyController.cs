using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    

    public Rigidbody rBody;
    public GameObject deathEffect;

    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 60f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    private Vector3 currentTarget;
    private Vector3 lastKnownPos;

    public bool lineOfSight = false;
    public bool idleActive = false;
    public float alertSpeed;

    public NavMeshAgent agent;
    public Animator animator;

    [SerializeField]
    State state = State.roam;

    private Vector2 mapBounds;
    public Vector3 destination;

    public float walkSpeed;
    public float wanderRadius = 10f;
    public float waitChance = 0.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 5f;
    private float waitTime;

    void Start()
    {
        currentTarget = GameObject.FindGameObjectWithTag("Player").gameObject.transform.position;
        rBody = GetComponent<Rigidbody>();
        walkSpeed = Random.Range(4.0f, 8f);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        maxWaitTime = Random.Range(5, 15);

        mapBounds = new Vector2(GameObject.FindGameObjectWithTag("Ground").transform.localScale.x, GameObject.FindGameObjectWithTag("Ground").transform.localScale.z);

        state = State.roam;
        SetRandomDestination();
    }

    void Update()
    {
        DetectTargets();
        switch (state)
        {
            case State.roam:
                MoveStateUpdate();
                break;
            case State.idle:
                WaitStateUpdate();
                break;
            case State.seek:
                Seek();
                break;
        }
    }

    public void Seek(Transform target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, walkSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
    public void Seek()
    {
       // transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, walkSpeed * Time.deltaTime);
       if (lineOfSight)
        {
            transform.LookAt(currentTarget);
            agent.SetDestination(currentTarget);
        }
        state = State.roam;
    }

    public void Die()
    {
       // Instantiate(deathEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void DetectTargets() 
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (Collider target in targetsInViewRadius)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    Debug.Log(target.name + " is in vision!");
                    state = State.seek;
                    lineOfSight = true;
                    lastKnownPos = target.transform.position;
                    currentTarget = lastKnownPos;
                }
                else
                {
                    Debug.Log("out of sight");
                    agent.SetDestination(lastKnownPos);
                    state = State.roam;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewRadius;
        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }

    void WaitStateUpdate()
    {
        // Do nothing while waiting
       // animator.SetBool("Walk1", false);
       // animator.SetBool("Idle1", true);
    }

    void SetRandomDestination()
    {
        Vector3 randomPoint = Random.insideUnitSphere * wanderRadius;
        randomPoint += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, 1);

        destination = hit.position;
        agent.SetDestination(destination);
       // animator.SetBool("Walk1", true);
    }

    IEnumerator WaitForSecondsAndChangeState(float seconds)
    {
       // animator.SetBool("Idle1", true);
        yield return new WaitForSeconds(seconds);
       // animator.SetBool("Idle1", false);
       // animator.SetBool("Walk1", true);
        state = State.roam;
        SetRandomDestination();
    }

    bool WaitRandom()
    {
        return Random.value < waitChance;
    }

    void MoveStateUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            state = WaitRandom() ? State.idle : State.roam;

            if (state == State.roam)
            {
                SetRandomDestination();
            }
            else
            {
                waitTime = Random.Range(minWaitTime, maxWaitTime);
                StartCoroutine(WaitForSecondsAndChangeState(waitTime));
            }
        }
    }
}

enum State
{
    roam,
    suspicious,
    alerted,
    seek,
    attack,
    stunned,
    idle,
    decide
}