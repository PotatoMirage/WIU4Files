using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Aim")]
public class Aim_Action : Action
{
    private readonly int aimAnimHash = Animator.StringToHash("Aim");

    [Header("Aim Settings")]
    public float rotationSpeed = 5f;

    public override void OnEnter(StateController controller)
    {
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(aimAnimHash, 0.1f);
    }

    public override void Act(StateController controller)
    {
        if (controller.chaseTarget == null) return;

        // Rotate to face player
        Vector3 dir = controller.chaseTarget.position - controller.transform.position;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            controller.transform.rotation = Quaternion.RotateTowards(
                controller.transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime * 60f
            );
        }
    }
}