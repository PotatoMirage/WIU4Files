using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Fall")]
public class Fall_Action : Action
{
    private readonly int fallAnimHash = Animator.StringToHash("Fall");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    [Header("Fall Settings")]
    public float fallSpeed = 5f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    public override void OnEnter(StateController controller)
    {
        controller.navMeshAgent.enabled = false;

        
        controller.rigidBody.isKinematic = false;
        controller.rigidBody.useGravity = true;

        if (controller.animator != null)
        {
            controller.animator.SetFloat(speedParamHash, 0f);
            controller.animator.CrossFadeInFixedTime(fallAnimHash, 0.1f);
        }
    }

    public override void Act(StateController controller)
    {
        
    }

    public override void OnExit(StateController controller)
    {
        controller.rigidBody.linearVelocity = Vector3.zero;
        controller.rigidBody.angularVelocity = Vector3.zero;
        controller.rigidBody.isKinematic = true;
        controller.rigidBody.useGravity = false;
    }
}