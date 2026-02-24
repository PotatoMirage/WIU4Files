using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/IsAimComplete")]
public class IsAimComplete_Decision : Decision
{
    public float waitTime = 1.5f;
    public float angleThreshold = 180f;

    public override bool Decide(StateController controller)
    {
        if (controller.chaseTarget == null) return false;

        bool timerDone = controller.CheckIfCountDownElapsed(waitTime);

        Vector3 dir = controller.chaseTarget.position - controller.transform.position;
        float angle = Vector3.Angle(controller.transform.forward, dir);
        bool isFacing = angle < angleThreshold;

        return timerDone && isFacing;
    }
}