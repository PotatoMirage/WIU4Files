using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Death")]
public class Death_Action : Action
{
    private readonly int deathAnimHash = Animator.StringToHash("Death");
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
            controller.animator.CrossFadeInFixedTime(deathAnimHash, 0.1f);
        }

        EnemyVFX vfx = controller.GetComponent<EnemyVFX>();
        if (vfx != null)
            vfx.PlayDeathEffect();
    }

    public override void Act(StateController controller)
    {
        
    }
}