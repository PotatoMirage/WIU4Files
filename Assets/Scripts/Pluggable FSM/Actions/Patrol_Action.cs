using UnityEngine;
using UnityEngine.AI;
[CreateAssetMenu(menuName = "Actions/Patrol")]
public class Patrol_Action : Action
{
    private readonly int speedParamHash = Animator.StringToHash("Speed");
    private readonly int locomotionAnimHash = Animator.StringToHash("Locomotion");

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float patrolRadius = 5f;

    [Header("Idle Behaviour")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.speed = patrolSpeed;
        controller.navMeshAgent.isStopped = false;
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(locomotionAnimHash, 0.1f);
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
        isWaiting = true;
    }

    public override void Act(StateController controller)
    {
        // Smooth speed blend instead of snapping
        if (controller.animator != null)
        {
            float currentSpeed = controller.navMeshAgent.velocity.magnitude;
            controller.animator.SetFloat(speedParamHash, currentSpeed, 0.1f, Time.deltaTime);
        }

        if (isWaiting)
        {
            // Stand still and count down
            controller.navMeshAgent.isStopped = true;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                controller.navMeshAgent.isStopped = false;
                Vector3 randomPoint = RandomNavSphere(controller.transform.position, patrolRadius, -1);
                controller.navMeshAgent.SetDestination(randomPoint);
            }
        }
        else
        {
            // Moving - check if we arrived
            if (controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance
                && !controller.navMeshAgent.pathPending)
            {
                // Arrived, start waiting
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}