using UnityEngine;

[CreateAssetMenu(menuName = "Actions/WallUpDown")]
public class WallUpDown_Action : Action
{
    private readonly int upDownAnimHash = Animator.StringToHash("UpDown");

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float pointReachedThreshold = 0.2f;

    private bool movingToTop = false;

    public override void OnEnter(StateController controller)
    {
        if (controller.animator != null)
            controller.animator.CrossFadeInFixedTime(upDownAnimHash, 0.1f);

        // Start by moving to bottom
        movingToTop = false;
    }

    public override void Act(StateController controller)
    {
        if (controller.treeTopPoint == null || controller.treeBottomPoint == null) return;

        Vector3 target = movingToTop ?
            controller.treeTopPoint.position :
            controller.treeBottomPoint.position;

        target.x = controller.transform.position.x;
        target.z = controller.transform.position.z;

        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        float dist = Vector3.Distance(controller.transform.position, target);
        if (dist <= pointReachedThreshold)
            movingToTop = !movingToTop;
    }
}