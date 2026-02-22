using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/IsAimComplete")]
public class IsAimComplete_Decision : Decision
{
    public float angleThreshold = 10f;

    public override bool Decide(StateController controller)
    {
        if (controller.chaseTarget == null) return false;

        Vector3 dir = controller.chaseTarget.position - controller.transform.position;
        float angle = Vector3.Angle(controller.transform.forward, dir);
        return angle < angleThreshold;
    }
}