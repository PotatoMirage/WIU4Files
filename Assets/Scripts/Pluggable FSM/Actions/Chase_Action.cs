using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Chase")]
public class Chase_Action : Action
{
    private readonly int speedParamHash = Animator.StringToHash("Speed");
    private readonly int locomotionAnimHash = Animator.StringToHash("Locomotion");
    [Header("Movement")]
    public float chaseSpeed = 5f;

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.speed = chaseSpeed;
        controller.navMeshAgent.isStopped = false;
        controller.navMeshAgent.updatePosition = true;
        controller.navMeshAgent.updateRotation = true;
        if (controller.navMeshAgent.isOnNavMesh)
        {
            controller.navMeshAgent.Warp(controller.transform.position);
        }
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(locomotionAnimHash, 0.1f);
    }
    public override void Act(StateController controller)
    {
        if (controller.chaseTarget == null) return;
        controller.navMeshAgent.destination = controller.chaseTarget.position;
        if (controller.animator != null)
            controller.animator.SetFloat(speedParamHash, controller.navMeshAgent.velocity.magnitude);
    }
}