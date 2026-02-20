using UnityEngine;

[CreateAssetMenu(menuName = "Decisions/Distance")]
public class Distance_Decision : Decision
{
    [Tooltip("The distance at which this decision returns true.")]
    public float radius = 5f;

    public override bool Decide(StateController controller)
    {
        if (controller.chaseTarget == null) return false;

        float distance = Vector3.Distance(controller.transform.position, controller.chaseTarget.position);

        // Returns true if the target is inside the radius
        return distance <= radius;
    }
}