using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Hit")]
public class Hit_Action : Action
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");
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
            controller.animator.CrossFadeInFixedTime(hitAnimHash, 0.1f);
        }
    }

    public override void Act(StateController controller)
    {
        
    }
}