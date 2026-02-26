using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Actions/Fall")]
public class Fall_Action : Action
{
    private readonly int fallAnimHash = Animator.StringToHash("Fall");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    [Header("Fall Settings")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    public float navMeshSnapDistance = 5f;

    [Header("Fall Movement")]
    public float forwardForce = 3f;
    public float downwardForce = 2f;

    private bool hasLanded = false;

    public override void OnEnter(StateController controller)
    {
        hasLanded = false;

        controller.navMeshAgent.enabled = false;
        controller.rigidBody.isKinematic = false;
        controller.rigidBody.useGravity = true;
        controller.rigidBody.freezeRotation = true;

        Vector3 fallDirection = controller.transform.forward * forwardForce +
                                Vector3.down * downwardForce;
        controller.rigidBody.AddForce(fallDirection, ForceMode.Impulse);

        if (controller.animator != null)
        {
            controller.animator.SetFloat(speedParamHash, 0f);
            controller.animator.CrossFadeInFixedTime(fallAnimHash, 0.1f);
        }
    }

    public override void Act(StateController controller)
    {
        if (hasLanded) return;

        // Cast down to check for ground
        if (Physics.Raycast(controller.transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.1f, groundLayer))
        {
            // Try snapping to nearest NavMesh
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSnapDistance, NavMesh.AllAreas))
            {
                controller.transform.position = navHit.position;
                controller.navMeshAgent.enabled = true;

                // Reset physics
                controller.rigidBody.isKinematic = true;
                controller.rigidBody.useGravity = false;
                controller.rigidBody.freezeRotation = false;

                // Transition to appropriate active state (chase player, not idle/aim)
                if (controller.chaseTarget != null)
                {
                    controller.TransitionToState(controller.currentState != null ? controller.currentState : controller.remainState);
                }
                else
                {
                    controller.TransitionToState(controller.remainState); // fallback
                }

                hasLanded = true;
            }
            else
            {
                // Landed off NavMesh — stay in fall/ragdoll
                Debug.LogWarning($"Spider landed off NavMesh at {hit.point}. Staying in fall state.");
                hasLanded = true; // prevent repeated checks
            }
        }
    }

    public override void OnExit(StateController controller)
    {
        if (!controller.rigidBody.isKinematic)
        {
            controller.rigidBody.linearVelocity = Vector3.zero;
            controller.rigidBody.angularVelocity = Vector3.zero;
        }

        controller.rigidBody.freezeRotation = false;
        controller.rigidBody.isKinematic = true;
        controller.rigidBody.useGravity = false;
    }
}