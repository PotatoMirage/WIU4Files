using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/IsLandComplete")]
public class IsLandComplete_Decision : Decision
{
    public float rotationThreshold = 1f;
    public float minLandTime = 0.5f;

    public override bool Decide(StateController controller)
    {
        // Don't check until minimum time has passed
        if (controller.stateTimeElapsed < minLandTime) return false;

        float xAngle = Mathf.Abs(controller.transform.eulerAngles.x);
        float zAngle = Mathf.Abs(controller.transform.eulerAngles.z);

        if (xAngle > 180f) xAngle = 360f - xAngle;
        if (zAngle > 180f) zAngle = 360f - zAngle;

        return xAngle < rotationThreshold && zAngle < rotationThreshold;
    }
}