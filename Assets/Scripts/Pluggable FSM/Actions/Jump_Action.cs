using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Actions/Jump")]
public class Jump_Action : Action
{
    [Header("Jump Settings")]
    public float jumpDuration = 1f;
    public float jumpHeight = 4f;

    private readonly int jumpAnimHash = Animator.StringToHash("Jump");
    private readonly int locomotionAnimHash = Animator.StringToHash("Locomotion");

    public override void OnEnter(StateController controller)
    {
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(jumpAnimHash, 0.1f);

        // Turn off NavMesh interference
        controller.navMeshAgent.autoTraverseOffMeshLink = false;
        controller.navMeshAgent.updatePosition = false;
        controller.navMeshAgent.updateRotation = false;

        EnemyVFX vfx = controller.GetComponent<EnemyVFX>();
        if (vfx != null)
            vfx.PlayJumpTakeoff();
    }

    public override void Act(StateController controller)
    {
        if (controller.navMeshAgent.isOnOffMeshLink)
        {
            OffMeshLinkData data = controller.navMeshAgent.currentOffMeshLinkData;
            float normalizedTime = controller.stateTimeElapsed / jumpDuration;
            if (normalizedTime < 1.0f)
            {
                Vector3 currentPos = Vector3.Lerp(data.startPos, data.endPos, normalizedTime);

                // handles flat gaps, jumping up, and dropping down
                float heightDiff = data.endPos.y - data.startPos.y;
                bool isDropping = heightDiff < -0.5f;
                bool isJumpingUp = heightDiff > 0.5f;

                float arcHeight;
                if (isJumpingUp)
                    arcHeight = jumpHeight + heightDiff; // extra height to clear the ledge
                else if (isDropping)
                    arcHeight = jumpHeight * 0.2f;
                else
                    arcHeight = jumpHeight; // flat gap, normal arc

                currentPos.y += arcHeight * Mathf.Sin(normalizedTime * Mathf.PI);

                controller.transform.position = currentPos;
                Vector3 direction = (data.endPos - data.startPos).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                    controller.transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                controller.transform.position = data.endPos;
                controller.navMeshAgent.CompleteOffMeshLink();
                controller.navMeshAgent.Warp(data.endPos);
                controller.navMeshAgent.updatePosition = true;
                controller.navMeshAgent.updateRotation = true;
                controller.stateTimeElapsed = jumpDuration;

                EnemyVFX vfx = controller.GetComponent<EnemyVFX>();
                if (vfx != null)
                    vfx.PlayJumpLand();
            }
        }
    }

    public override void OnExit(StateController controller)
    {
        // Always make sure these are re-enabled on exit
        controller.navMeshAgent.updatePosition = true;
        controller.navMeshAgent.updateRotation = true;
        controller.navMeshAgent.autoTraverseOffMeshLink = true;
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(locomotionAnimHash, 0.1f);
    }
}