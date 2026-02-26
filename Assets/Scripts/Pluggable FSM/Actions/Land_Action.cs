using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Land")]
public class Land_Action : Action
{
    private readonly int landAnimHash = Animator.StringToHash("Land");
    private readonly int speedParamHash = Animator.StringToHash("Speed");

    [Header("Land Settings")]
    public float rotationSpeed = 5f;
    public float rotationThreshold = 1f; // degrees

    private Quaternion targetRotation;

    public override void OnEnter(StateController controller)
    {
        if (controller.animator != null)
        {
            controller.animator.SetFloat(speedParamHash, 0f);
            controller.animator.CrossFadeInFixedTime(landAnimHash, 0.1f);
        }

        // Target is flat on the ground - no wall tilt
        targetRotation = Quaternion.Euler(0f, controller.transform.eulerAngles.y, 0f);
        controller.isOnWall = false;
    }

    public override void Act(StateController controller)
    {
        // Smoothly rotate from wall orientation to ground orientation
        controller.transform.rotation = Quaternion.RotateTowards(
            controller.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime * 60f
        );
    }

    public override void OnExit(StateController controller)
    {
        controller.transform.rotation = targetRotation;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(controller.transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            controller.navMeshAgent.enabled = true;
            controller.transform.position = hit.position;
            controller.navMeshAgent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning("Spider landed too far from NavMesh, agent not enabled!");
        }
    }
}