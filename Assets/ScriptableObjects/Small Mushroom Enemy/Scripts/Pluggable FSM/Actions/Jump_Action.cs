using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Jump")]
public class Jump_Action : Action
{
    private readonly int jumpAnimHash = Animator.StringToHash("Jump");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.isStopped = true;
        controller.navMeshAgent.velocity = Vector3.zero;
        controller.animator.SetFloat(speedParamHash, 0f);
        controller.animator.CrossFadeInFixedTime(jumpAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        
    }
}