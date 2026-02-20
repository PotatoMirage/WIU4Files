using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Death")]
public class Death_Action : Action
{
    private readonly int deathAnimHash = Animator.StringToHash("Death");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.isStopped = true;
        controller.navMeshAgent.velocity = Vector3.zero;
        controller.animator.SetFloat(speedParamHash, 0f);
        controller.animator.CrossFadeInFixedTime(deathAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        
    }
}