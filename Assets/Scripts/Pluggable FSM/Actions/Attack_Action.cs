using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Attack")]
public class Attack_Action : Action
{
    private readonly int attackAnimHash = Animator.StringToHash("Attack");
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
            controller.animator.CrossFadeInFixedTime(attackAnimHash, 0.1f);
        }
    }

    public override void Act(StateController controller)
    {
        // Constantly look at the player while attacking
        Vector3 dir = controller.chaseTarget.position - controller.transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            controller.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}