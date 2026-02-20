using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Actions/Patrol")]
public class Patrol_Action : Action
{
    private readonly int speedParamHash = Animator.StringToHash("Speed");
    private readonly int locomotionAnimHash = Animator.StringToHash("Locomotion");

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.isStopped = false;

        // Force the animator back to the movement state!
        controller.animator.CrossFadeInFixedTime(locomotionAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        controller.animator.SetFloat(speedParamHash, controller.navMeshAgent.velocity.magnitude);

        if (controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance && !controller.navMeshAgent.pathPending)
        {
            Vector3 randomPoint = RandomNavSphere(controller.transform.position, 10f, -1);
            controller.navMeshAgent.SetDestination(randomPoint);
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