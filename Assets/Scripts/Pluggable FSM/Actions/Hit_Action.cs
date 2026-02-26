using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Hit")]
public class Hit_Action : Action
{
    private readonly int hitAnimHash = Animator.StringToHash("Hit");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    [Header("States to return to")]
    public State wallReturnState;    // assign Spider_Aim
    public State groundReturnState;  // assign Spider_Chase

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

        EnemyVFX vfx = controller.GetComponent<EnemyVFX>();
        if (vfx != null)
            vfx.PlayHitEffect();
    }

    public override void Act(StateController controller) { }
}