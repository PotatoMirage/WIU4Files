using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Chase")]
public class Chase_Action : Action
{
    private readonly int speedParamHash = Animator.StringToHash("Speed");
    private readonly int locomotionAnimHash = Animator.StringToHash("Locomotion");

    public override void OnEnter(StateController controller)
    {
        // Ensure the agent is allowed to move when entering this state
        controller.navMeshAgent.isStopped = false;

        // Force the animator back to the movement state!
        controller.animator.CrossFadeInFixedTime(locomotionAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        if (controller.chaseTarget == null) return;

        // follow target and update animation speed
        controller.navMeshAgent.destination = controller.chaseTarget.position;
        controller.animator.SetFloat(speedParamHash, controller.navMeshAgent.velocity.magnitude);
    }
}