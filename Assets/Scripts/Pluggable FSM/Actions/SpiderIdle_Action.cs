using UnityEngine;

[CreateAssetMenu(menuName = "Actions/SpiderIdle")]
public class SpiderIdle_Action : Action
{
    private readonly int idleAnimHash = Animator.StringToHash("Idle");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    public override void OnEnter(StateController controller)
    {
        if (controller.navMeshAgent != null && controller.navMeshAgent.enabled)
        {
            controller.navMeshAgent.isStopped = true;
            controller.navMeshAgent.velocity = Vector3.zero;
        }

        if (controller.animator != null)
        {
            controller.animator.SetFloat(speedParamHash, 0f);
            controller.animator.CrossFadeInFixedTime(idleAnimHash, 0.1f);
        }
    }

    public override void Act(StateController controller) { }
    public override void OnExit(StateController controller) { }
}